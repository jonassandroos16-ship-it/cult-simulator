/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/CultSimulator/**/*.razor",
    "./src/CultSimulator/**/*.cs"
  ],
  theme: {
    extend: {
      colors: {
        void: {
          top: "#2e1065",
          mid: "#0f0a24",
          deep: "#050308",
        },
        altar: {
          core: "#4c1d95",
          mid: "#1e1b4b",
          edge: "#0f0a24",
        },
        gold: {
          DEFAULT: "#fbbf24",
          warm: "#f59e0b",
          deep: "#d97706",
          pale: "#fef3c7",
          soft: "#fde68a",
        },
        mystic: {
          DEFAULT: "#a78bfa",
          deep: "#7c3aed",
          text: "#ddd6fe",
          void: "#2e1065",
        },
        success: "#34d399",
        danger: "#fb7185",
        neutral: "#94a3b8",
      },
      fontFamily: {
        display: ['Cinzel', 'Georgia', 'Times New Roman', 'serif'],
        body: ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'sans-serif'],
      },
      animation: {
        "altar-pulse": "altar-pulse 3s ease-in-out infinite",
        "slow-rotate": "slow-rotate 8s linear infinite",
        "slow-rotate-20": "slow-rotate 20s linear infinite",
        "modal-rise": "modal-rise 0.4s cubic-bezier(0.34,1.56,0.64,1)",
        "fade-in": "fade-in 0.3s ease",
        "float-up": "float-up 1s ease-out forwards",
      },
      keyframes: {
        "altar-pulse": {
          "0%, 100%": { opacity: "0.5", transform: "scale(1)" },
          "50%": { opacity: "1", transform: "scale(1.1)" },
        },
        "slow-rotate": {
          from: { transform: "rotate(0deg)" },
          to: { transform: "rotate(360deg)" },
        },
        "modal-rise": {
          from: { transform: "translateY(30px) scale(0.95)", opacity: "0" },
          to: { transform: "translateY(0) scale(1)", opacity: "1" },
        },
        "fade-in": {
          from: { opacity: "0" },
          to: { opacity: "1" },
        },
        "float-up": {
          "0%": { transform: "translateY(0)", opacity: "1" },
          "100%": { transform: "translateY(-60px)", opacity: "0" },
        },
      },
      backdropBlur: {
        glass: "16px",
      },
    },
  },
  plugins: [
    require("@tailwindcss/forms"),
  ],
};
