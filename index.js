const core = require('@actions/core');
const github = require('@actions/github');
const fetch = require('node-fetch');
var base64 = require('js-base64').Base64;
const { Octokit } = require('@octokit/core');
const { createPullRequest } = require('octokit-plugin-create-pull-request');
const MyOctokit = Octokit.plugin(createPullRequest);

async function run() {
    try {
        const repo_token = core.getInput('repo-token');
        const pat_token = core.getInput('token');
        const comment = core.getInput('comment', { required: false });

        var auth = await get_deepprompt_auth(pat_token);
        var auth_token = auth['access_token'];
        var session_id = auth['session_id'];

        if (comment) {
            const pr_body = core.getInput('pr-body');
            const comment_id = core.getInput('comment-id');
            console.log(comment_id);
            const pr_number = core.getInput('pr-number');
            const repo = core.getInput('repo');
            const repo_url = `https://github.com/${repo}`;
            const session_id = pr_body.split('Session ID: ')[1].split('.')[0];

            const query = comment.split('/devbot ')[1];
            const response = await get_response(auth_token, session_id, query);
            post_comment(repo_token, repo_url, pr_number, response);
        } else {
            const issue_title = core.getInput('issue-title');
            const issue_body = core.getInput('issue-body');
            const issue_number = core.getInput('issue-number');

            const issue_metadata = JSON.parse(issue_body);
            const buggy_file_path = issue_metadata['buggy_file_path'];
            const repo_url = issue_metadata['repo_url'];
            var file = await get_file(repo_token, repo_url, buggy_file_path);

            var fixed_file = await fix_bug(auth_token, session_id, file, issue_metadata['start_line_number'], issue_metadata['bottleneck_call']);
            
            console.log(fixed_file);
            
            create_pr(repo_token, repo_url, buggy_file_path, issue_title, issue_number, file, fixed_file, session_id);
        }
    } catch (error) {
        core.setFailed(error.message);
    }
}

async function post_comment(access_token, repo_url, pr_number, comment)
{
    var owner = repo_url.split('/')[3];
    var repo_name = repo_url.split('/')[4];
    var url = `https://api.github.com/repos/${owner}/${repo_name}/issues/${pr_number}/comments`;
    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/vnd.github.v3+json',
            'Authorization': `token ${access_token}`
        }, 
        body: JSON.stringify({
            'body': comment
        })
    });
    let data = await response.json();
    console.log(data);
}

async function get_response(auth_token, session_id, query)
{
    var url = 'https://data-ai-dev.microsoft.com/deeppromptdev/api/v1/query';
    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'DeepPrompt-Version': 'v1',
            'Accept': 'application/json',
            'Authorization': `Bearer ${auth_token}`,
            'DeepPrompt-Session-ID': session_id
        },
        body: JSON.stringify({
            'query': query,
        })
    });
    let data = await response.json();
    let response_text = data['response_text'];
    console.log("---------------");
    console.log(data);
    return response_text;
}

async function fix_bug(auth_token, session_id, buggy_code, start_line_number, buggy_function_call)
{
    var url = 'https://data-ai-dev.microsoft.com/deeppromptdev/api/v1/query';
    var intent = 'perf_fix';
    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'DeepPrompt-Version': 'v1',
            'Accept': 'application/json',
            'Authorization': `Bearer ${auth_token}`,
            'DeepPrompt-Session-ID': session_id
        },
        body: JSON.stringify({
            'query': 'Can you fix the above perf issue?',
            'intent': intent,
            'context': {
                'source_code': buggy_code,
                'buggy_function_call': buggy_function_call,
                'start_line_number': start_line_number.toString(),
                'prompt_strategy': 'instructive'
            }
        })
    });
    let data = await response.json();
    let fix = data['response_text'].slice(0, -3).split('```csharp\n\n')[1];

    let end_line_number = find_end_of_function(buggy_code, start_line_number);
    var lines = buggy_code.split('\n');
    var fixed_lines = lines.slice(0, start_line_number - 1).concat(fix.split('\n')).concat(lines.slice(end_line_number + 1));
    fix = fixed_lines.join('\n');

    console.log("---------------");
    console.log(fix);
    return fix;
}

async function get_deepprompt_auth(access_token) {
    try {
        url = 'https://data-ai-dev.microsoft.com/deeppromptdev/api/v1/exchange'
        let response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                'token': access_token,
                'provider': 'github'
            })
        });
        let auth_token = await response.json();
        return auth_token;
    }
    catch (error) {
        core.setFailed(error.message);
    }
}

async function get_file(access_token, repo_url, buggy_file_path) {
    const user = repo_url.split('/')[3];
    const repo = repo_url.split('/')[4];
    try {
        url = `https://api.github.com/repos/${user}/${repo}/contents/${buggy_file_path}`
        let response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `token ${access_token}`
            }
        });
        let data = await response.json();
        let file = base64.decode(data.content);
        return file;
    }
    catch (error) {
        core.setFailed(error.message);
    }
}

function find_end_of_function(code, start_line_number) {
    var lines = code.split('\n');
    var i = start_line_number;
    var open_braces = 0;
    while (i < lines.length) {
        var line = lines[i];
        for (var j = 0; j < line.length; j++) {
            if (line[j] == '{') {
                open_braces++;
            }
            else if (line[j] == '}') {
                open_braces--;
            }
        }
        if (open_braces == 0) {
            return i;
        }
        i++;
    }
    return i;
}

async function create_pr(access_token, repo_url, buggy_file_path, issue_title, issue_number, file, fixed_file, session_id) {
    const user = repo_url.split('/')[3];
    const repo = repo_url.split('/')[4];
    const fix_title = `PERF: Fix ${issue_title}`;
    const branch_name = 'test-branch-' + (new Date()).getTime();

    const octokit = new MyOctokit({
        auth: access_token,
    });

    var change = {}
    change[buggy_file_path] = fixed_file;
    octokit.createPullRequest({
        owner: user,
        repo: repo,
        title: fix_title,
        body: `Auto-generated PR fixing issue #${issue_number}. Session ID: ${session_id}.`,
        head: branch_name,
        base: 'main',
        update: false,
        forceFork: false,
        changes: [
            {
                files: change,
                commit: fix_title,
            },
        ],
    });
}

run();
