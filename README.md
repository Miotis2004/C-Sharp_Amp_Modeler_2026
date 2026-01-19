# AmpModeler (C# Neural Amp Modeler)

## Overview

**AmpModeler** is a from-scratch neural amp modeling application written in **C#**, designed as a standalone Windows application with a long-term path toward plugin formats (VST3/AU).

The project intentionally avoids existing amp modeling frameworks (JUCE, NAM, etc.) and instead focuses on a clean, modular architecture that separates:

* Real-time audio hosting
* DSP graph and signal processing
* Neural inference
* Capture and dataset generation
* User interface

The primary goals are **real-time stability**, **architectural clarity**, and **behavioral accuracy**, rather than rapid feature delivery.

---

## High-Level Architecture

```
UI (WinUI 3)
 |
AmpModeler.Engine
 |
DSP Graph
 |--> Amp Model (ML)
 |--> Cab IR (Convolution)
 |
IAudioHost (Abstractions)
 |--> WASAPI Host
 |--> ASIO Host
```

The DSP and engine layers are completely independent of the UI and audio backend, enabling reuse in future plugin or offline rendering scenarios.

---

## Solution Structure

```
AmpModeler.sln
 ├─ AmpModeler                (WinUI 3 Standalone App)
 ├─ AmpModeler.Core           (Low-level DSP & realtime primitives)
 ├─ AmpModeler.Engine         (Audio engine & DSP graph orchestration)
 ├─ AmpModeler.Audio.Abstractions
 ├─ AmpModeler.Audio.Wasapi
 ├─ AmpModeler.Audio.Asio
 ├─ AmpModeler.DspBlocks
 ├─ AmpModeler.ML
 └─ AmpModeler.Capture
```

Each project has a single, intentional responsibility.

---

## Project Responsibilities

### AmpModeler (WinUI 3 App)

**Role:** Standalone application host

Responsibilities:

* Application lifecycle
* Audio backend selection (WASAPI / ASIO)
* Engine startup and shutdown
* UI binding to parameters
* Metering, controls, preset browsing

Non-responsibilities:

* DSP logic
* Audio thread code
* Model inference logic

This project should remain *thin*.

---

### AmpModeler.Core

**Role:** Realtime-safe foundations

Responsibilities:

* Audio buffer structures
* DSP interfaces (`IAudioBlock`, etc.)
* Parameter definitions and smoothing
* Utility math and helpers
* Realtime safety rules

Design constraints:

* No UI dependencies
* No audio I/O dependencies
* No ML runtime dependencies
* No allocations or locks in realtime paths

This is the lowest-level layer and should remain minimal and stable.

---

### AmpModeler.Engine

**Role:** Audio engine and DSP graph orchestration

Responsibilities:

* Fixed-block audio processing
* DSP block chaining and routing
* Buffer adaptation between hosts and engine
* Parameter propagation into DSP blocks
* Engine state management

This project owns **how audio flows**, but not **how audio is captured or displayed**.

---

### AmpModeler.Audio.Abstractions

**Role:** Audio host interface definitions

Responsibilities:

* `IAudioHost`
* Audio device settings and descriptors
* Backend-agnostic audio callback contracts

This layer allows:

* WASAPI and ASIO to be interchangeable
* Future plugin or offline hosts
* Testing with mock hosts

---

### AmpModeler.Audio.Wasapi

**Role:** Windows WASAPI implementation

Responsibilities:

* WASAPI exclusive/shared mode setup
* Device enumeration
* Buffer marshaling into engine format

Dependencies:

* NAudio
* Audio.Abstractions
* Core

---

### AmpModeler.Audio.Asio

**Role:** ASIO implementation

Responsibilities:

* ASIO driver enumeration
* Low-latency buffer handling
* Driver-specific edge case handling

Dependencies:

* NAudio (ASIO wrapper)
* Audio.Abstractions
* Core

This project is intentionally isolated due to driver variability.

---

### AmpModeler.DspBlocks

**Role:** DSP processing modules

Responsibilities:

* Cabinet IR convolution
* Filters and EQ
* Dynamics (gate, limiter)
* Oversampling utilities
* Future non-ML amp stages if needed

This project contains **signal processing math**, not engine control logic.

---

### AmpModeler.ML

**Role:** Neural amp model inference

Responsibilities:

* ONNX Runtime integration
* Amp model loading
* Inference execution
* Model state handling

Training is intentionally performed **outside** the application and models are imported via ONNX.

This separation allows experimentation with different architectures without destabilizing the engine.

---

### AmpModeler.Capture

**Role:** Capture and dataset generation

Responsibilities:

* Excitation signal generation
* Playback/record capture sessions
* WAV file output
* Metadata serialization
* Capture validation metrics

This project supports repeatable, high-quality dataset creation for offline training.

---

## Design Principles

* No allocations in the audio thread
* No locks in the audio thread
* Fixed internal block size
* Clear ownership of buffers
* UI never touches realtime data directly
* Stability before sound quality
* Sound quality before features

---

## Roadmap (High-Level)

- [x] Stable WASAPI audio engine (Core, Engine, Host implementations complete)
- [x] UI Device Selection & Start/Stop (MVVM Integration)
- [ ] DSP graph and parameter system
- [ ] IR convolution cab block
- [ ] Capture tool MVP
- [ ] Neural amp model inference MVP
- [ ] Oversampling and feel refinement
- [ ] ASIO backend hardening
- [ ] Plugin architecture exploration (VST3/AU)

---

## Status

**Active Development**.

The foundational layers are complete:
*   **Core:** Zero-allocation audio buffers and processing interfaces.
*   **Engine:** Fixed-block adaptation logic (ring buffers) and orchestration.
*   **Audio Backend:** Low-latency WASAPI Shared Mode implementation (allocation-free hot path).
*   **UI:** Basic WinUI 3 interface with MVVM architecture for device selection and engine control.

Current focus: DSP Graph implementation.

---

## Disclaimer

This project is experimental and intended for research and learning. No guarantees of fitness for production use are provided.

---

## License

To be determined.
