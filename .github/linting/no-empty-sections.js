// Custom markdownlint rule: no-empty-sections
// Flags any heading immediately followed by another heading with no content between them.

module.exports = {
  names: ["no-empty-sections"],
  description: "Headings must have content before the next heading",
  tags: ["headings"],
  function: function rule(params, onError) {
    // Only apply this rule to files under docs/
    if (!params.name.includes("/docs/")) {
      return;
    }

    const tokens = params.tokens;
    let lastHeadingOpen = null;
    let hasContent = false;

    for (const token of tokens) {
      if (token.type === "heading_open") {
        // If we just saw a heading and found no content before this next heading, flag it
        // Skip H1 headings — a page title with no body before the first section is intentional
        if (lastHeadingOpen !== null && !hasContent && lastHeadingOpen.tag !== "h1") {
          onError({
            lineNumber: lastHeadingOpen.lineNumber,
            detail: `Heading on line ${lastHeadingOpen.lineNumber} has no content before the next heading on line ${token.lineNumber}`,
          });
        }
        lastHeadingOpen = token;
        hasContent = false;
      } else if (
        token.type !== "heading_close" &&
        token.type !== "inline" &&
        lastHeadingOpen !== null
      ) {
        // Any token that isn't part of the heading itself counts as content
        hasContent = true;
      }
    }
  },
};
