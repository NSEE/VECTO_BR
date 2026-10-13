VECTO-AirDrag: Changelog
===================
## v3.3.5 (2025-12-09) ##

### Bug fixes ###
- Support setting second_amdm_params via input file.

## v3.3.3 (2025-12-05) ##

### Bug Fixes ###
- Official support 2nd amendment parameters for 3rd amendment transitional period

## v3.3.2-RC (2025-12-02) ##

### Bug Fixes ###
- Support 2nd amendment parameters for 3rd amendment transitional period

## v3.3.1 (2025-08-26) ##

### Bug Fixes ###
- Fix tool version usage to 3.3.1

## v3.3.0 (2025-03-14) ##

Release of the VECTO Air Drag tool adhering to the certification regulation EU 2017/1400 as amended by (EU) 2025/258 of 7 February 2025. The tool has been updated to export new XML files and new measurement segment validity criteria. More specifically:

### Features ###
- Generate XSD v2.6 VECTO inputs
- Accept positive values for delta CFD
- Adjusts criteria and input to 3rd amendment updates:
  -  Change v_veh_avg_max_HS and v_veh_avg_min_HS criteria for buses
  - Change v_veh_avg_max_HS and v_veh_avg_min_HS criteria for lorries
  - Change t_amb_min criteria
  - Change v_wind_avg_max_HS and gust criteria
  - Read new required value s_brake from .csdat input
  - Add <s_brake> column and values to .csdat input
  - Break simulation if LowSpeed test exceeds 25 minutes
- Create package during Release build

### Refactor ###
- Do not hardcode VECTO references

## v3.2.0 (2022-05-21) ##

TUG improvements:

  * New version number
  * DriftCheck implemented
  * Drift corrected values in output files
  * Adaption of gearbox types. Old gearbox types are also valid
  * New vehicle classes added (MLr, MLvan, MB, HB)
  * Change of criteria parameter tq_sum_float_delta_LS dependent of vehicle class
  * Vehicle file corrected to new input data
  * Info labels for tq_sum_float_delta_LS in declaration mode added
  * New signal inputs <n_EM> and <n_wheel_ave>
  * Bugfix output path in the result file. Now the correct file is written to the title
  * Bugfix time sequence. Now the time is checked also with digits after the decimal separator
  * Bugfix preferences. Now in the preference GUI the unit for Log-file size is written
  * Update DemoData
  * Update ReleaseNotes

## v3.1.9 (2019-01-31) ##

TUG improvements:

  * New version number
  * Adaption of gearbox types. Old gearbox types are also valid
  * Update DemoData
  * Update ReleaseNotes

## v3.1.8 (2019-01-04) ##

TUG improvements:

  * New version number
  * Adaption of gearbox types on new legalisation
  * Adaption of GUIs to only show the logo from the European Commission
  * Update DemoData
  * Update ReleaseNotes
  * Update UserManual
  * Release candidate output

## v3.1.7 (2018-06-26) ##

TUG improvements:

  * New version number
  * Correction of the result output for gearbox type AT if n_eng is not given

## v3.1.6 (2018-02-15) ##

TUG improvements:

  * New version number
  * Hotfix for VECTO Input file
  * Culture info is automatically set to [en-US] standard

## v3.1.5 (2018-02-08) ##

TUG improvements:

  * New version number
  * Hotfix for rounding method

## v3.1.4 (2018-01-22) ##

TUG improvements:

  * New version number
  * Bugfix for rounding method

## v3.1.3 (2017-11-07) ##

TUG improvements:

  * New version number
  * Bugfix for speed criterium

## v3.1.2 (2017-11-03) ##

TUG improvements:

  * New version number
  * License module deleted

## v3.1.1 (2017-10-04) ##

TUG improvements:

  * New version number
  * Bugfix for direct start option (now the VECTO input files can be created)
  * Correction version number (now the version number is generated over the assembly number)

## v3.1.0 (2017-07-14) ##

TUG improvements:

  * New version number
  * Bugfix for correct detection of first/last datapoint inside of a MS
  * Update VECTO input file generation
  * Update VECTO hashing added
  * Update ReleaseNotes
  * Update UserManual

## v3.0.8 (2017-03-22) ##

TUG improvements:

  * New version number
  * Correction calc vehicle speed stability criteria
  * Update DemoData
  * Update ReleaseNotes
  * Update UserManual
  * Update JobFile

## v3.0.7 (2017-02-03) ##

TUG improvements:

  * New version number
  * Validity criterias for calibration run extended (see technical annex)
  * New calibration criterias (v_veh_1s_delta_CAL, v_veh_ave_delta_CAL)

## v3.0.5/6 (2017-01-13) ##

TUG improvements:

  * New version number
  * Change of needed digits after the decimal separator (DGPS: DD.DD[7]/MM.MM[5]; GPS: DD.DD[5]/MM.MM[3])
  * Uneven numbers of datasets per heading in calibration test now allowed
  * Correction of maximum allowed vehicle high from class 1 and class 2

## v3.0.3/4 (2016-11-25) ##

TUG improvements:

  * New version number
  * Bugfix for the criteria detection in calibration and LS/HS run
  * Correction of the output messages
  * Correction of the DemoData

## v3.0.1/2 (2016-11-25) ##

TUG improvements:

  * New version number
  * Bugfix for digit check by use of GPS
  * Bugfix for measurement section detection for root point calculation

## v3.0.0 (2016-11-16) ##

TUG improvements:

  * New version number
  * Update of needed signals in calibration run (t_ground no longer required as input)
  * A license valid until 16th of March is distributed with the code.
  * Update DEMOData

## v2.0.7-beta8/9 (2016-11-09) ##

TUG improvements:

  * Correction of rolling resistance criteria
  * Correction of digit test for coordinates

## v2.0.7-beta7 (2016-10-20) ##

TUG improvements:

  * New version number
  * Including Declaration and Engineering Mode to program and Job-File
  * Fv_veh calculation update (now option 2 is the new preferred one) Option 2 is deleted from result file
  * Uneven numbers of datasets per heading in HS test now allowed
  * Calculation of F_acc changed due to wheels inertia deletion (now F_acc = 1.03*m*a)
  * New design of criteria tab
  * Update Standard criteria values + renaming
  * Include of new variables for validity criteria (t_amb; t_ground; tq_grd)
  * gradient correction implemented
  * Check of digits after decimal separator for all coordinates, transmission ratios (gear + axle) and altitudes (Values in vehicle file changed to string values)
  * Vehicle file check included (For height and class code)
  * Expand vehicle file with vVehMax and GVMMax value
  * Change vehicle file configuration parameter from "Rigid/Tractor" to "No/Yes"
  * Displacement of Genshape file into code
  * Add reference vehicle high in genshape class
  * Add minimum/maximum vehicle high in genshape class
  * Control of min/max height with vehicle height only in Declaration mode
  * Calculation of new Result values (delta_CdxA_height; v_avg_LS/HS; t_amb_LS1, CdxA_H1/2, beta_H1/2)
  * Change of CdxA and beta calculation
  * Output files in Declaration mode extended
  * Output files adapted
  * Expansion of the job-File due to new results
  * vehWidth and wheelsInertia deleted from vehicle-file
  * Deletion of unused variables (omega_wh; omega_wh_acc; omega_p_wh_acc; t_tire; p_tire; ...)
  * Deletion of [ss.ss] coordinate input
  * Update DEMOData
  * Update of Excel DemoData file
  * Update user manual and release notes

## v2.0.6-beta6 (2016-01-21) ##

TUG improvements:

  * New version number
  * Bugfix control heading calculation in *.csms file
  * Bugfix old functions for day changes deleted
  * Update user manual and release notes

## v2.0.5-beta6 (2015-11-25) ##

TUG improvements:

  * New version number
  * Renaiming of VECTO-CSE into VECTO-Air Drag
  * Bugfix dist calculation
  * Bugfix calculation of a_veh_avg and omega_p_wh_acc
  * Update of the ending message if an error during the calculation detected
  * Update user manual and release notes

## v2.0.4-beta6 (2015-10-13) ##

TUG improvements:

  * New version number
  * Updates by error/warning output
  * Update user manual and release notes
  * Free definition for list and decimal separators in the CSV-Files
  * Variable definition of the coordinate unit (*.csms and *.csdat files)
  * Update of the excel file
  * Update error messages

## v2.0.3-beta6 (2015-07-23) ##

TUG improvements:

  * New version number
  * Bugfix calculation without additional signals
  * Update user manual and release notes (Now always the newest version will be opened)
  * Deleted unused variables (FC from input data, t_amb_tamac from criteria)
  * Add ground temperature as optional signal to input data (<t_ground>)
  * Add new criteria parameter t_ground_max = 40C
  * Set t_amb_max to 25C (old 35C)

## v2.0.2-beta6 (2015-07-20) ##

TUG improvements:

  * Bugfix heading control
  * Update user manual
  * Add "Report bug" option
  * Add JIRA Quick Start Guide
  * Delete valid_t_tire from output files and job-file
  * Change t_tire from required input data to optional (like p_tire)
  * Delete rho_air_ref from criteria and GUI

## v2.0.2-beta6 (2015-07-01) ##

TUG improvements:

  * New Version number.
  * Update region and language settings: now it is regardless of the system which settings are used ! But the file definitions are still the same
  * Update of the excel makro in terms of the system region and language settings

## v2.0.2-beta5 (2015-06-24) ##

Mostly TUG improvements:

  * FIX Job-save missing criteria (hack)
  * Update of calculation of the CdxA value from measured drag forces
  * Update GenShape File
  * Update of calibration of vehicle speed and anemometer speed (high speed test instead of "claibration test")
  * new criteria delta_n_eng for LS/HS included
  * Option in VECTO-CSE to read in cardan speed instead of engine speed and gear ratios for HS and LS
  --> Update of the vehicle file
  * Update calculation with cardan speed: Calculation only if engine speed is not given an automated transmission is used
  * Program user friendly updates
  * Update F_acc calculation
  * Update r_dyn calculation for each test (HS, LS1, LS2)
  * Update DemoData
  * Update user manual and release notes
  * Output folder will be now created automatically if its not existing without questioning
  * Welcome form added for first application start
  * Correction of welcome window
  * Add the Excel-makro to generate the input data into the DemoData files
  * New Version number.

## v2.0.1-beta5 (2015-05-21) ##

TUG improvements:

  * Handling of time steps where coordinates are constant over a certain time period (GPS accuracy issue) set gradient to 0 to avoid division by zero
  * Allow also non-continuous input data in *.csdat files
  * Direct start option included
  * Anemometer instrument calibration removed from CSE calculation
  * Update of definition of beta-signal (180 = air flow from front)
  * Acceleration correction: true as default
  * Control if heading and given direction is always the same
  * set criteria to "standard" when CSE opens
  * store information when switching between tabs
  * ResetCriteria and ImportCriteria do not clear main tab
  * New Version number.

## v2.0.1-beta4 (2014-11-13) ##

TUG improvements:

  * Bugfix linear regression calculation LS
  * Add control for beta angle <= 360 and calculation to +-180 if angle > +-180
  * Update user manual
  * New Version number.

## v2.0.1-beta3 (2014-11-10) ##

TUG improvements:

  * Correction HS calculation
  * Correction length calculation for the first section in the dat-file when there is a gap between the first and second MS.
  * Update length calculation in csms-file to check if specified length from user and coordinates are the same
  * New Version number.

## v2.0.1-beta2 (2014-09-30) ##

TUG improvements:

  * Correction GenShape Check
  * New Version number.

## v2.0.1-beta1 (2014-09-15) ##

TUG improvements:

  * IO: Data reading corrected for first data set in measurement section file, weather file and data files.
  * New Version number.

TODv2.0(O: 2014-06-25) .1
--------------------
Mostly JRC contributions (see VECTO-29 & VECTO-35):

  * IO: JSON-ize preferences, vehicle, job & criteria-files EXCEPT from Track-file.
  * IO: CSVize all the rest files with a single header line and use '#' for comment lines.
  * IO: Separate config/ from Declaration/ folders.
  * UI: Provide default-values and help-messages in GUI/files with infos fetched from JSON-schemas.
  * UI: Make the Log-window visible at all times (more necessary now that unhandled exceptions are appropriately reported).
  * Log: Gather all unhandled exceptions and report them into log-window and log-file.
  * Log: Improving error-reporting by including stack-traces and timestamps into the log-file, for post-mortem examination.
  * Translate all file-paths against `Prefs.WorkingDir`, so that i.e. Job-files can be ported to other computers.
  * Possible to specify any editor (not only notepad.exe) for viewing files.
  * Standarize versioning using [SemanticVersioning](http://semver.org/).
  * Welcome developers and users with README.md, CHANGES.md and COPYING.txt files.
### Internal:
  * Implement an API for writing Header/Body json-files.
  * Apply Object-oriented design weith resource-management when I/O files.
  * Sporadic fixes to work with filenames having 2-part extensions (ie `some_file.csjob.json`).
  * Log: Improve logging-API so now a single log-routine is used everywhere(instead of 3 different ones).
  * async: Stop abusing worker-Thread with Globals, use DoWorkEventArgs instead.
  * async: Start using Exceptions instead of CancelAsync() and error-flags.
  * General restructuring of the folders and names in the project.



More analytically:

## v2.0.1-beta0 (2014-06-23) ##

Mostly TUG improvements:

  * json: Store run-results within the Job-file.
  * csv: Ensure result-files are valid CSVs.
  * csv: Changed comment symbol in CSV files from 'c' --> '#'
  * csv: Unify hunits into header labels.
### Internal:
  * Use Exceptions instead of CancelAsync() and error-flags in calc-routines and input.vb.
  * Remove unused distVincenty() func.
  * Added EUPL preamble on all source-files.


## v2.0.1-pre2 (2014-06-04) ##

JRC contributions:

  * Convert Job & Criteria files to JSON and possible to store them separately.
  * Still supporting old format for reading.
  * Use Use WorkingDir trick for all job-file paths, so that Job-files can be ported to other computers.
  * UI: Make the Log-window visible at all times (more necessary now that unhandled exceptions are appropriately reported).
  * UI: Setup criteria-infobox from JSON-schema.
### Internal:
  * Log unhandled exceptions.
  * Gather all infos related to Job-properties (type, description, units) in a single place, the JSON-schema for the job-file.
  * async: Stop abusing worker-Thread with Globals, use DoWorkEventArgs instead.
  * Sporadic fixes to work with filenames having 2-part extensions (ie `some_file.csjob.json`).
  * More refactorings to simplify structure of source files and folders.


## v2.0.1-pre1 (2014-05-30) ##

JRC contributions:

  * Read/write Vehicle-file as JSON.
  * prefsUI: Add Reload button.
  * Remember window-location (use .net Settings for that).
  * All logs (even those sent to msg-box) are written to log-file, with timestamps and stack-traces.
### Internal:
  * Start saving stack-traces into the log-file.
  * Enhance JSON-files with standard header/body behavior.
  * Link JSON to GUI controls (labels & toolstips)
  * json: Read defaults from schemas.
  * Rework logging as a single routine, whether invoked from Background Worker or not.


## v2.0.1-pre0 (2014-05-23) ##

JRC contributions:

  * Separate config/ from Declaration/ folders.
  * Remove the versioning infos from app-name (manual, project-name, folders) and
    use [SemanticVersioning](http://semver.org/) 2.0.0 instead.
  * Possible to use any editor (not only notepad.exe).
  * Added README.md, CHANGES.md, COPYING.txt files.
### Internal:
  * Auto create config/ on the 1st run, converted to JSON with transparent error-handling.
  * FIX leaking of file-descriptors by using VB's 'Using' statement (class 'cFile_v3' now implements IDisposeable).


## 2014-05-14: CSE2.01 ##

1st delivery from TU-Graz under Lot-3.
