export async function valueToTreeObject(octokit, owner, repo, path, value) {
    let mode = "100644";
    if (value !== null && typeof value !== "string") {
        mode = value.mode || mode;
    }
    // Text files can be changed through the .content key
    if (typeof value === "string") {
        return {
            path,
            mode: mode,
            content: value,
        };
    }
    // Binary files need to be created first using the git blob API,
    // then changed by referencing in the .sha key
    const { data } = await octokit.request("POST /repos/{owner}/{repo}/git/blobs", {
        owner,
        repo,
        ...value,
    });
    const blobSha = data.sha;
    return {
        path,
        mode: mode,
        sha: blobSha,
    };
}
