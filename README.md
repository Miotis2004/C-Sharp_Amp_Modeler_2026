# C-Sharp_Amp_Modeler_2026

Alt-Neural Amp Modeler (C# / Standalone)
Overview

This project is a from-scratch neural amp modeling application built in C# as a standalone Windows application, with a long-term goal of supporting plugin formats (VST3/AU).

The system is inspired by modern neural amp modeling approaches but does not use existing amp modeling frameworks. Instead, it implements its own real-time audio engine, DSP graph, capture workflow, and neural inference pipeline, while allowing the use of:

A selectable audio I/O backend (WASAPI or ASIO)

A machine learning runtime (ONNX Runtime) for real-time inference

The project emphasizes real-time stability, extensibility, and architectural clarity over rapid feature delivery.

Core Goals

Real-time, low-latency audio processing suitable for guitar input

Neural network–based amp head modeling

Separate cabinet simulation via impulse response convolution

Clean separation between audio host, DSP engine, and UI

Standalone application first, plugin support later without rewriting DSP code

Educational and exploratory focus without reliance on JUCE, NAM, or similar frameworks

Key Features (Planned)
Audio Engine

Selectable audio backend:

WASAPI (via NAudio)

ASIO (via NAudio ASIO wrapper)

Fixed internal processing block size with host buffer adaptation

Lock-free, allocation-free audio callback

CPU and dropout monitoring

DSP Graph

Modular block-based processing chain

Deterministic real-time execution

Parameter smoothing and automation support

Expandable routing model

Amp Modeling

Neural amp head modeling using ONNX Runtime

Causal, real-time safe architectures (e.g., TCN-style models)

Optional oversampling for aliasing reduction

Model state handling for dynamic behavior

Cabinet Simulation

Partitioned convolution IR engine

Hot-swappable impulse responses with click-free transitions

Stereo and mono IR support

Capture Workflow

Integrated excitation playback and response recording

Metadata-rich capture sessions

Exportable datasets for offline training

Validation metrics (levels, noise floor, alignment)

Non-Goals

This project intentionally does not aim to:

Compete immediately with commercial amp modelers

Reimplement low-level OS audio drivers

Provide DAW hosting features

Ship preset packs or licensed models

The focus is on engineering correctness and sound behavior, not rapid monetization.

Architecture Overview
UI
 |
 |--> Audio Backend Selector (WASAPI / ASIO)
 |
IAudioHost
 |
Realtime Audio Engine
 |
DSP Graph
 |--> Input Conditioning
 |--> Noise Gate
 |--> Neural Amp Model
 |--> Post EQ
 |--> Cab IR Convolution
 |--> Output Limiter


All DSP logic lives in a core library that is independent of the UI and audio backend, enabling future reuse in plugins or offline renderers.

Technology Stack

Language: C#

Framework: .NET (latest LTS)

Audio I/O: NAudio (WASAPI + ASIO)

ML Runtime: Microsoft.ML.OnnxRuntime

Training: External (Python / PyTorch recommended)

Platform: Windows (initially)

Project Structure (Planned)
/src
  /Core
    AudioEngine
    DspGraph
    Parameters
    Utilities
  /AudioHosts
    WasapiHost
    AsioHost
  /DspBlocks
    AmpModelBlock
    CabConvolver
    Filters
    Dynamics
  /Capture
    Excitation
    Recording
    SessionMetadata
  /UI
    ViewModels
    Views
/docs
/models
/ir

Development Philosophy

No allocations in the audio thread

No locks in the audio thread

Clear ownership of buffers and state

Predictable CPU usage

Measure before optimizing

Stability before sound quality

Sound quality before features

Roadmap (High-Level)

Stable real-time audio engine (WASAPI)

DSP graph and parameter system

IR convolution cab block

Capture tool MVP

Neural amp inference MVP

Oversampling and feel refinements

ASIO backend

Plugin architecture exploration

Status

Early development. Expect breaking changes, incomplete features, and evolving architecture.

License

License to be determined. This repository is currently intended for research, learning, and experimentation.

Disclaimer

This project is experimental and intended for educational and research purposes. Use at your own risk. No warranties are provided.