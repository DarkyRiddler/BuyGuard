import { createProxyMiddleware } from 'http-proxy-middleware';
import { NextApiRequest, NextApiResponse } from 'next';
import { NextApiHandler } from 'next';
import { IncomingMessage, ServerResponse } from 'http';

const API_URL = 'https://localhost:7205';

const proxy = createProxyMiddleware({
  target: API_URL,
  changeOrigin: true,
  pathRewrite: {
    '^/api': '', // usuwamy `/api` z początku ścieżki
  },
});

export const config = {
  api: {
    bodyParser: false, // ważne – pozwala przepuścić body do backendu
  },
};

const handler: NextApiHandler = async (req: NextApiRequest, res: NextApiResponse) => {
  return new Promise((resolve) => {
    proxy(req as unknown as IncomingMessage, res as unknown as ServerResponse, () => {
      resolve(null);
    });
  });
};

export default handler;
