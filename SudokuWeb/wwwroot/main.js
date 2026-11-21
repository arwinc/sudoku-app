// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './_framework/dotnet.js'

const outElement = document.getElementById('out');
const loadingElement = document.getElementById('loading');
const inputRow = document.getElementById('input-row');
const cmdInput = document.getElementById('cmd');

const MAX_INPUTS = 2;
let pendingResolve = null;
let currentColor = '';
let isBold = false;

const ansiColors = {
  '30': '#000000', '31': '#f44747', '32': '#6a9955', '33': '#d7ba7d',
  '34': '#569cd6', '35': '#c586c0', '36': '#4ec9b0', '37': '#d4d4d4',
  '90': '#808080', '91': '#ff6b6b', '92': '#89d185', '93': '#ffe066',
  '94': '#75beff', '95': '#e599f7', '96': '#66d9ef', '97': '#ffffff'
};

let currentLine = null;

function trimOldInputs() {
  const inputs = outElement.querySelectorAll('div[data-input="true"]');
  // Once there are more than MAX_INPUTS, remove everything up to the oldest kept input
  if (inputs.length > MAX_INPUTS) {
    const cutoffInput = inputs.item(inputs.length - MAX_INPUTS);
    while (outElement.firstChild && outElement.firstChild !== cutoffInput) {
      outElement.removeChild(outElement.firstChild);
    }
  }
}

function getCurrentLine() {
  if (!currentLine) {
    currentLine = document.createElement('div');
    outElement.appendChild(currentLine);
  }
  return currentLine;
}

function appendAnsiText(text, isUserInput = false) {
  const parts = text.split(/(\x1b\[[0-9;]*m)/g);

  for (const part of parts) {
    if (!part) continue;

    const match = part.match(/^\x1b\[([0-9;]*)m$/);
    if (match) {
      const codes = match.at(1).split(';');
      for (const code of codes) {
        if (code === '' || code === '0' || code === '39') {
          currentColor = '';
          isBold = false;
        } else if (code === '1') {
          isBold = true;
        } else if (code === '22') {
          isBold = false;
        } else if (ansiColors[code]) {
          currentColor = ansiColors[code];
        }
      }
    } else {
      const lines = part.split('\n');
      for (let i = 0; i < lines.length; i++) {
        if (lines[i].length > 0) {
          const lineDiv = getCurrentLine();
          if (isUserInput) {
            lineDiv.dataset.input = 'true';
          }
          const span = document.createElement('span');
          span.textContent = lines[i];
          if (currentColor) span.style.color = currentColor;
          if (isBold) span.style.fontWeight = 'bold';
          lineDiv.appendChild(span);
        } else if (i < lines.length - 1) {
          getCurrentLine().appendChild(document.createTextNode('\u200B'));
        }

        if (i < lines.length - 1) {
          currentLine = null;
        }
      }
    }
  }

  if (isUserInput) {
    trimOldInputs();
  }

  window.scrollTo(0, document.body.scrollHeight);
}

cmdInput.addEventListener('keydown', (e) => {
  if (e.key === 'Enter' && pendingResolve) {
    const value = cmdInput.value;
    cmdInput.value = '';
    inputRow.style.display = 'none';
    loadingElement.style.display = 'block';
    appendAnsiText(`> ${value}\n`, true);
    const resolve = pendingResolve;
    pendingResolve = null;
    resolve(value);
  }
})

const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet
  .withDiagnosticTracing(false)
  .create();

outElement.textContent = "";

setModuleImports('main.js', {
  dom: {
    appendOutput: (text) => appendAnsiText(text),
    readLineAsync: () => new Promise((resolve) => {
      pendingResolve = resolve;
      loadingElement.style.display = 'none';
      inputRow.style.display = 'flex';
      cmdInput.focus();
      window.scrollTo(0, document.body.scrollHeight);
    }),
    onExit: () => {
      loadingElement.style.display = 'none';
      inputRow.style.display = 'none';
      document.getElementById('restart-btn').style.display = 'inline-block';
    }
  }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

// run the C# Main() method and keep the runtime process running and executing further API calls
await runMain();