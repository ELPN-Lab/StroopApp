# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),  
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## Unreleased

### Added
- **Calibration task (QUEST+)**: a Stroop profile can run as a calibration task. The task estimates the presentation time at which the participant reaches a target accuracy (85 % by default).
  - Phase 1 presents a fixed number of trials at a fixed presentation time (20 trials at 2000 ms by default).
  - Phase 2 is adaptive. QUEST+ chooses each presentation time between a lower and an upper bound (140 to 5000 ms by default). The phase stops when the estimate converges or when the maximum number of trials is reached (40 to 100 trials by default).
  - The profile editor has a "Calibration task" checkbox. When it is checked, the editor shows the calibration settings grouped by phase.
  - The experimenter dashboard shows the current calibration phase and presentation time.
  - The end page shows a result card with the convergence status, the estimated time (t50) and its 90 % interval (t5 to t95). A details window adds a slider that reads the presentation time at another accuracy.
  - The Excel export adds a "Calibration" worksheet. It holds one row per calibration block, with the result, the slider reading and all the parameters.
  - The experimenter can stop a calibration block at any time. The result is computed from the trials played so far.
  - The calibration math is checked against an independent Python implementation, described in `StroopApp.XUnitTests/Services/Calibration/Reference/README.md`.

### Planned Features

#### Serial Port Communication for MRI/EEG Synchronization
**Status**: Prototype created, awaiting needs analysis

**Background**:  
Initial development explored integration with MRI/EEG equipment via serial port triggers. The goal was to synchronize stimulus presentation (fixation cross, visual cues, colored words) with external recording devices.

**Challenges encountered**:
- No access to MRI equipment for real-world testing and validation
- Unclear requirements for trigger timing and configuration
- Complex parameter space requiring domain expertise:
  - When to send triggers? (pre-stimulus, post-stimulus, on-response?)
  - Configurable delays (e.g., 50ms before stimulus onset)
  - Event types to transmit (trial start, stimulus type, response, block boundaries)

**Status for v1.1.0**:  
Not included due to insufficient needs analysis and lack of testing infrastructure.

**Next steps**:
- Conduct needs analysis with researchers using MRI/EEG in Stroop studies
- Define essential configuration parameters
- Develop testable implementation with simulated serial port
- Validate with real MRI/EEG equipment before inclusion

**Questions to resolve with research community**:
- What trigger timing is standard in your field?
- Which stimulus events require external synchronization?
- What configuration flexibility is needed?

---

#### Multi-Screen Window Management
- Choose which screen to open participant/experimenter windows
- Remember window positions between sessions
- Easier window positioning (currently requires dragging invisible title bar or Windows keyboard shortcuts)

#### Customizable Instructions
- Create custom instruction pages
- Add/remove instruction screens
- Per-profile instruction templates

#### Export Menu UX Redesign
- Clearer export/quit application workflow
- Simplified end-of-experiment actions

#### Additional Experiment Parameters
- Support for more than 4 colors
- Customizable color palette
- Extended trial types

#### CSV Export Format
- Universal compatibility for data analysis tools

#### Documentation
- Expanded troubleshooting guides
- Video tutorials
- Community-contributed experiment templates

---

### Technical Improvements
- Enhanced error recovery mechanisms
- Optimized memory usage for very large datasets (10,000+ trials)
- Windows Defender compatibility improvements

---

## [1.1.0] - 2026-04-06

### Changed
- **MVVM architecture**: migrated state management and ViewModels to CommunityToolkit.Mvvm, replacing manual `INotifyPropertyChanged` implementations with source generators (`[ObservableProperty]`, `[RelayCommand]`).
- **View architecture**: migrated Views from constructor-injected UserControls to a `DataTemplate` + `ContentControl` pattern, with a dedicated `DataTemplates.xaml` registered in `App.xaml` `MergedDictionaries`.
- **ViewModel modularization**: exposed sub-ViewModels as public properties in parent ViewModels; added `GraphsViewModel` to own and dispose graph sub-VMs.
- **ExportPath synchronization**: fixed via `Settings.PropertyChanged` subscription.
- **Disposal chain**: fixed in end-experiment flow (`IDisposable`, `Unloaded` event).
- **Layout**: fixed layout issues in `ProgressView`.

### Fixed
- Restore step added to CI test workflow to prevent missing dependency failures.
- Publish workflow now restores with `win-x64` RID for correct self-contained builds.

---

## [1.0.0] - 2026-02-13

### Added
- First stable public release of **StroopApp**.
- Stroop task execution with visual cues and keyboard responses.
- Participant and experiment profile management.
- Real-time reaction time visualization.
- Multi-language user interface (English / French).
- Export of experiment results to Excel (`.xlsx`).
- GitHub Actions CI with Release build and executable artifact generation.
- `AppConfiguration` model for centralized config directory injection.
- `IPageFactory` / `PageFactory` for DI-based page resolution.
- `INavigationAware` interface for automatic per-window `NavigationService` injection.

### Fixed
- Removal of obsolete domain and infrastructure code:
  - Deprecated `Result` model and related unit tests.
  - Unused user notification abstraction (`IUserNotifier`, `DefaultNotifier`).
- Improved error reporting with clearer and localized messages (EN / FR).
- Cleanup of unit tests to follow modern xUnit assertions.
- Reduced build noise by addressing non-actionable test warnings.

### Changed
- **Dependency Injection**: all services (`ProfileService`, `ParticipantService`, `KeyMappingService`, `ExportationService`, `TrialGenerationService`) are now registered in the DI container and injected via constructors — no more manual `new Service(configDir)` in pages.
- **Navigation architecture**: `NavigationService` is no longer a DI singleton. Each window (`ExperimentWindow`, `ParticipantWindow`) creates its own instance using the shared `IPageFactory`. Experimenter pages receive their window's `NavigationService` automatically via `INavigationAware`.
- **Experimenter pages** (`ConfigurationPage`, `ExperimentDashBoardPage`, `EndExperimentPage`) are registered as Transient in DI and resolved via `NavigateTo<T>()`.
- **ViewModel cleanup**: removed unused constructor parameters (`windowManager`, `languageService`, `settings`) that were only passed through to page constructors.
- **XAML binding optimization**: added `Mode=OneTime` to 11 static bindings in `ExperimentProgressView.xaml` and `EndExperimentPage.xaml` for properties that don't change during the page lifetime.
- Improved internal code organization (enum extraction, clearer responsibilities).
- Updated resource files to better reflect current application behavior.
- Minor internal refactors with no functional impact.

### Notes
- The application is considered stable and suitable for research and educational use.
- Some dependencies (e.g. charting components) rely on release-candidate versions; this is documented and considered acceptable for v1.
- Nullable reference warnings and large-scale refactors (e.g. `SharedExperimentData` decomposition) are intentionally deferred to post-v1 iterations.

---

[1.1.0]: https://github.com/MylanBeghin/StroopApp/releases/tag/v1.1.0
[1.0.0]: https://github.com/MylanBeghin/StroopApp/releases/tag/v1.0.0
