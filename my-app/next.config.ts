import type { NextConfig } from "next";

import path from "path";

const nextConfig: NextConfig = {
  sassOptions: {
    includePaths: [
      path.join(__dirname, "node_modules"),
      path.join(__dirname, "node_modules/bootstrap/scss"),
    ],
  },
};

export default nextConfig;
