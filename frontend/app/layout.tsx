import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Ball Sort — On-Chain Puzzle Game",
  description: "Sort the colored balls. Powered by Solana & MagicBlock Ephemeral Rollups.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
