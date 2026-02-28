/**
 * FormAtlas Figma Plugin — UI (ui.ts) entry point.
 * Wires plugin UI interactions to worker protocol messages.
 * This file is bundled separately as the HTML UI context.
 */

import type { UiToWorkerMessage, WorkerToUiMessage, ImportOptions } from "./protocol";
import { DEFAULT_IMPORT_OPTIONS } from "./protocol";

// ---- State ----
let currentOptions: ImportOptions = { ...DEFAULT_IMPORT_OPTIONS };
let jsonText = "";
let pngBytes: Uint8Array | undefined;

// ---- Wire message handler ----
window.onmessage = (event: MessageEvent) => {
  const msg = event.data.pluginMessage as WorkerToUiMessage;
  if (!msg) return;

  if (msg.type === "VALIDATE_RESULT") {
    if (msg.ok) {
      showStatus(`✓ Valid bundle: ${msg.form?.name} (${msg.counts?.nodes} nodes)`);
    } else {
      showStatus(`✗ Validation failed: ${msg.error ?? "Unknown error"}`, true);
    }
    return;
  }

  if (msg.type === "IMPORT_PROGRESS") {
    showStatus(`${msg.phase}: ${msg.done}/${msg.total}`);
    return;
  }

  if (msg.type === "IMPORT_RESULT") {
    if (msg.ok) {
      showStatus(`✓ Import complete. Created ${msg.createdNodeIds?.length ?? 0} layers.`);
    } else {
      showStatus(`✗ Import failed: ${msg.error ?? "Unknown error"}`, true);
    }
    return;
  }
};

function postMessage(msg: UiToWorkerMessage): void {
  parent.postMessage({ pluginMessage: msg }, "*");
}

function showStatus(text: string, isError = false): void {
  const el = document.getElementById("status");
  if (el) {
    el.textContent = text;
    el.style.color = isError ? "red" : "green";
  }
}

// ---- UI event wiring (called after DOM ready) ----
document.addEventListener("DOMContentLoaded", () => {
  const jsonInput = document.getElementById("json-input") as HTMLTextAreaElement | null;
  const pngInput = document.getElementById("png-input") as HTMLInputElement | null;
  const validateBtn = document.getElementById("validate-btn");
  const importBtn = document.getElementById("import-btn");

  jsonInput?.addEventListener("change", () => {
    jsonText = jsonInput.value;
  });

  pngInput?.addEventListener("change", async () => {
    const file = pngInput.files?.[0];
    if (file) {
      const buf = await file.arrayBuffer();
      pngBytes = new Uint8Array(buf);
    }
  });

  validateBtn?.addEventListener("click", () => {
    if (!jsonText) { showStatus("No JSON loaded.", true); return; }
    postMessage({ type: "VALIDATE_REQUEST", jsonText });
  });

  importBtn?.addEventListener("click", () => {
    if (!jsonText) { showStatus("No JSON loaded.", true); return; }
    postMessage({ type: "IMPORT_REQUEST", jsonText, ...(pngBytes !== undefined && { pngBytes }), options: currentOptions });
  });
});
