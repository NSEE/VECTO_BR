# VECTO 5\.x Release Notes

![](img%5CRelease%20Notes%20Vecto4x0.png)

<!-- Cover Slide -->



# VECTO v5.0.8-RC (16-12-2025)


## Features

- multiple powertrains completed and single bus. (vecto/vecto!516)

- Single-gear IEPC in multiple powertrains (vecto/vecto!524)


## Bug Fixes

- Vecto console references (vecto/vecto!507)

- Completed bus input generation for v2.7 (vecto/vecto!508)

- DefaultDriverStrategy - add accel action after gearshift + roll + brake if overload (vecto/vecto!510)

- Add retarder losses to TC max torque request (vecto/vecto!511)

- Rename constructor parameter to match factory method (vecto/vecto!512)

- Propagate 'allowDeprecated' parameter for XMLReader classes through injection hierarchy (vecto/vecto!513)

- Multiple powertrain FCHV bus execution (vecto/vecto!515)

- vsum fields for Retarder, Axlegear for multiple powertrains. (vecto/vecto!517)

- Increase max iteration in InterpolateSearch (vecto/vecto!514)

- use of battery SoC limits (input vs default) (vecto/vecto!519)

- Create fuel cell in testpowertrain (required in shift strategy) (vecto/vecto!520)

- Added missing function to DummyRunNinjectModule (vecto/vecto!521)

- Calculation of axlegear efficiency for P4 vehicles: in case of an P4, use EM-in to calculate axlegear efficiency instead of Brake-in (vecto/vecto!522)

- Added multiple-powertrain sample jobs (vecto/vecto!525)

- Avoid null reference in DeclarationData.cs (vecto/vecto!526)

- bugs regarding multiple powertrains. (vecto/vecto!527)

- Modified Torque Converter columns to handle multiple powertrains. (vecto/vecto!528)

- OVC-HEV mode for FCHV vehicles, null reference in fuel cells. (vecto/vecto!529)

- Don't allow unsupported variations of multiple powertrain vehicles. (vecto/vecto!530)

- Enable unsupported vehicles in development mode (vecto/vecto!531)


## Refactor

- Refactored multiple powertrains builder methods (vecto/vecto!509)

- Reduced build-time warnings (vecto/vecto!518)

- Project vecto sw3/task 2/unit tests amdm3 (vecto/vecto!460)

# VECTO v5.0.7 Official Release (14-10-2025)


## Bug Fixes

- Release notes link in GUI dialog (vecto/vecto!480)

- Battery only mode for IHPC (vecto/vecto!481)

- Add System.Data.SqlClient to VECTO.vbproj (vecto/vecto!482)

- Treat 'not applicable' as 'none' in HeatPumpTypeDriverCompartmentType (vecto/vecto!483)

- Correct parsing of fuel cell in interim file (vecto/vecto!484)

- Proper mapping for ADC loss-map in XML (vecto/vecto!485)

- Improved error message for missing battery SoC bounds (vecto/vecto!486)

- VETO main version in dialogs (vecto/vecto!488)

- Corrected XSD dynamic charging types (vecto/vecto!491)

- Introduce new mod data postprocessing for BO-HEV (vecto/vecto!489)

- Copy monitoring data from input (for completed vehicles) (vecto/vecto!494)

- Generic vehicles engineering mode VTP files (vecto/vecto!492)

- Surround post-mortem analysis with try/catch block (vecto/vecto!495)

- Iepc gearshift (vecto/vecto!487)

- Iepc gearshift torque reserve (post MR !487) (vecto/vecto!496)

- Corrected name for Tyre in VIF (vecto/vecto!497)

- Corrected json sample file (vecto/vecto!498)

- Added missing bindings for mod-data post processing (vecto/vecto!499)

- Allow VTP with v2.4 vehicles. (vecto/vecto!503)

- Updated some sample vehicles to v2.7 (vecto/vecto!504)

- Display version dynamically (vecto/vecto!490)

- Gen veh FCHV values and DI (vecto/vecto!506)


## Refactor

- Remove redundant code (vecto/vecto!493)
# Release/v5.0.6-RC (22-09-2025)


## Features

- Disable v2.4 jobs (vecto/vecto!458)

- Update multistep GUI to work with new XSDs (vecto/vecto!469)

- Multiple axles partial implementation (vecto/vecto!471)


## Bug Fixes

- Made FuelCell Minpower, Maxpower optional (vecto/vecto!456)

- Engine-only simulation (vecto/vecto!457)

- EM data in PHEV rundata creation (vecto/vecto!459)

- Addressed vulnerability issues (vecto/vecto!328)

- Lifetime ranges in reports for PEV, HEV-OVC. (vecto/vecto!461)

- Work-around in ranges to make tests succeed (vecto/vecto!462)

- Respect job's battery SoC limits (vecto/vecto!463)

- VTP generic vehicles (vecto/vecto!465)

- Changed v2.6 XSD to allow DeltaCdxA_declared and DeltaTransferredCdxA value: zero (vecto/vecto!472)

- Avoid cyclic refs from !473 (vecto/vecto!475)

- Added WheelEnd info to MRF (vecto/vecto!476)

- Angledrive mod data, and PWheel axlegear efficiency (vecto/vecto!477)

- Disable engineering mode for multiple powertrains (vecto/vecto!478)

- VectoException using reference (vecto/vecto!479)


## Refactor

- Update VECTO to NET 8 (vecto/vecto!467)

- old .NET references (vecto/vecto!468)

- MultistepTool deprecated views (vecto/vecto!470)

- Remove unnecessary usings and nugets (vecto/vecto!473)


## Fix

- Extend Accelerate condition after xEV Overload (vecto/vecto!466)
# XMLConverterTool/5.0.4 (04-09-2025)


## Features

- Readers for v2.7 vehicle XSD, and support for fuel cell vehicles. (vecto/vecto!341)

- Read monitoring data from job (vecto/vecto!345)

- 3rd amendment mrf cif xml schemas (vecto/vecto!340)

- MRF v1.0 vehicle (lorries and FCHV primary buses) writers (vecto/vecto!354)

- CIF v1.0 vehicle part (v2.4 vehicles and v2.7 lorries) (vecto/vecto!355)

- Use monitoring data from job to write report (vecto/vecto!360)

- In motion charging postprocessing (vecto/vecto!344)

- Disable (for RC & official) v27 vehicles except H2-ICE & FCHV lorries (vecto/vecto!375)

- Readers for v2.7 buses, improved reader tests. (vecto/vecto!382)

- HEV - Get Best dSOC in vsum (vecto/vecto!383)

- 3rd amendment reports for buses (vecto/vecto!421)

- V1.0 reports for multiple powertrain lorries (vecto/vecto!395)

- Add Diesel B100 CI fuel (vecto/vecto!399)

- MRF and Monitoring report for multiple-powertrain primary buses (vecto/vecto!410)

- battery only mode for P2 (vecto/vecto!425)

- Run simulation for H2-ICE bus (primary + completed) (vecto/vecto!432)

- FCHV bus simulation, primary & completed (vecto/vecto!433)

- Single-bus mode for FCHV (vecto/vecto!435)

- Enable all v2.7 vehicles (vecto/vecto!436)

- VTP input and formulas for buses and trucks (vecto/vecto!424)

- XMLConversionTool can convert to version v2.7 (+ bugfixes) (vecto/vecto!451)

- Update jobs in Generic Vehicles to version v2.7 (vecto/vecto!453)


## Bug Fixes

- Non-https link in manual (vecto/vecto!339)

- V2.7 reader & XSD (vecto/vecto!342)

- Updated XSLT file and hashing code for new vehicles and components. (vecto/vecto!338)

- Lock StoredResults list before accessing it to avoid race condition (vecto/vecto!343)

- V27 vehicle issues (vecto/vecto!346)

- 882 merge artifacts (vecto/vecto!347)

- FCHV angledrive input (vecto/vecto!348)

- Modify schema so that results can be written compatible with results for 2nd amendment: (vecto/vecto!349)

- Restore deleted code in monitoring report (vecto/vecto!350)

- Typo in MRF Inject module (vecto/vecto!351)

- Proper namespace for VIF IEPC sub-element (vecto/vecto!352)

- Mockup tests run successfully (vecto/vecto!353)

- Correcting errors in XML schema (and sample files): no engine output in... (vecto/vecto!356)

- Bugfixes/updates for the Monitoring report and testing via the MockupTests. (vecto/vecto!357)

- Replace U+2013 by regular dashes (vecto/vecto!359)

- Added missing IMC testdata (vecto/vecto!361)

- Retarder compulsory in all MRF vehicle components. (vecto/vecto!362)

- Add further condition to decide which results to write in case the input data is a Multistep bus (vecto/vecto!363)

- Check Articulated in json vehicle (vecto/vecto!365)

- Segment in Bus AirDrag data creation (vecto/vecto!364)

- FCHV pre-run execution (vecto/vecto!366)

- Write ZeroCO2EmissionsRange and HydrogenRange to H2-ICE reports. (vecto/vecto!367)

- Remove wrong angledrive restrictions (vecto/vecto!369)

- Set vectorundata in completed bus results, (vecto/vecto!368)

- Simulate OVC for FCHVs (vecto/vecto!370)

- FCHV H2 range in reports (vecto/vecto!371)

- Architecture in some MRF v1.0 tests (vecto/vecto!372)

- H2 properties check in exempted vehicle input (vecto/vecto!373)

- FCHV input classes inheritance (vecto/vecto!374)

- Standard values enum entry for v2.6 (vecto/vecto!376)

- Add Driving Actions for IEPC gearshift (vecto/vecto!377)

- Of v2.7, allow only IMC, H2-ICE conventional, FCHV Lorries (vecto/vecto!384)

- Disable reading data from external csv (vecto/vecto!385)

- OVC results (vecto/vecto!387)

- Ignore FCHV pre-run in best deltaSoC calculation (vecto/vecto!388)

- FCHV engineering run (vecto/vecto!391)

- H2 check in bus job (vecto/vecto!392)

- Auxiliaries REESS connection (vecto/vecto!397)

- Read PowerOutputConsumptionMap as kW (vecto/vecto!401)

- Select pruned missions for FCHV primary bus (vecto/vecto!403)

- Use angledrive in lorries' gearshift data creation (vecto/vecto!404)

- Remove wrong bus angledrive restrictions (vecto/vecto!405)

- Decl GUI error message when FCHV in eng mode (vecto/vecto!407)

- Set NgTankSystem default for primary buses (vecto/vecto!408)

- FCHV F-IEPC simulation runs! (vecto/vecto!409)

- Operational range for group 10 vehicle weights (vecto/vecto!402)

- Ovc fc weighting to correspond to CO2 computation (vecto/vecto!413)

- 3s Buffer compute max EM PLoss for FL OPs (vecto/vecto!412)

- Conditioning power demand for FCHVs (vecto/vecto!414)

- Avoid em conditioning calculation for non-FCHV (vecto/vecto!416)

- Safe conditioning data lookup (vecto/vecto!417)

- Exception when getting MaxWindowsSize (vecto/vecto!422)

- For FCHV, APT-S/P gearboxes simulated as APT-N. (vecto/vecto!423)

- Ovc s-hev cs cd (vecto/vecto!420)

- Convert property Type to Architecture in axle powertrains (vecto/vecto!393)

- 3 job types for multiple powertrains (vecto/vecto!394)

- Removed NgTankSystem from Multiple_SHEV primary bus (vecto/vecto!398)

- Remove Retarder component from X4 architectures (vecto/vecto!400)

- Use multiple factory methods for Retarder and Angledrive data providers (vecto/vecto!406)

- Added FCHV missing gearbox bindings (vecto/vecto!427)

- Airdrag element in VIF report. (vecto/vecto!430)

- Do not require SoC limits for HV non-OVC (vecto/vecto!431)

- B100 density to 890 kg/m3 (vecto/vecto!411)

- Run old VIFs with v2.7 Completed vehicles (vecto/vecto!434)

- 1065 vehicle co2 group (vecto/vecto!428)

- DoCoast - add drive condition for overload (vecto/vecto!426)

- Generic retarder and failing tests (vecto/vecto!437)

- IEPC data adaptation (vecto/vecto!438)

- IHPC VECTO run data (vecto/vecto!439)

- FCHV IEPC rundata gearbox creation (vecto/vecto!440)

- Generic vehicles that failed to run (vecto/vecto!441)

- WHRCharger creation (vecto/vecto!442)

- Update wheelEnd sample (vecto/vecto!443)

- FCHV files in engineering mode (vecto/vecto!444)

- Initialize MaxChargingPower from static data if not available in input (vecto/vecto!445)

- FCHV battery and CD and CS runs (vecto/vecto!446)

- FCHV unit tests (vecto/vecto!448)

- XMLConversionTool bug fixes, more test cases, refactoring. (vecto/vecto!452)

- Bug for FCHV bus. (vecto/vecto!454)

- Conversion Tool ovcHev bug, affected Generic Vehicles (vecto/vecto!455)


## Refactor

- Merge refactoring branch (vecto/vecto!419)

- Merging refactorings from SW3 project to (vecto/vecto!396)

- Fchv iterative run strategy (vecto/vecto!449)


## Drop

- MaxChargingPower requirement for OVC in v2.7 (vecto/vecto!358)
# Nuget/v0.11.4-DEV (08-04-2025)


## Features

- Partial implementation for new vehicle battery (vecto/vecto!337)
# Release/v0.15.0-DEV (08-04-2025)


## Features

- New (v2.7) XSD for vehicles. (vecto/vecto!334)


## Bug Fixes

- Updated Monitoring Report XSD (FCHV, Multiple powertrains) (vecto/vecto!335)
# Release/v0.11.4-DEV (02-04-2025)


## Features

- IEPC with multiple load curves (#855) (vecto/vecto!321)

- H2 ICE vehicle in declaration mode (vecto/vecto!325)


## Bug Fixes

- Added error message for unknown completed vehicle missions (vecto/vecto!330)

- Segment lookup method (vecto/vecto!331)

- FullLoadCurves proper initialization (vecto/vecto!332)

- Manage 'GetTruckSegment' exception behavior (vecto/vecto!333)
# Release/4.3.3 (04-03-2025)


## Bug Fixes

- Check if XML element is signed (#950) (vecto/vecto!314)

- NgTankSystem optional for HEV lorries MRF XSD (vecto/vecto!315)


## Documentation

- Update XSD parameter IDs documentation (vecto/vecto!327)
# Release/4.3.2-RC (06-02-2025)


## Bug Fixes

- ATShiftStrategyOptimized - No UpshiftFomL if not locked (vecto/vecto!301)

- Forbid downshift to locked gear in APT-S if it generates direct upshift condition (vecto/vecto!307)

- Add condition to write BusAuxiliaries output data in vsum (vecto/vecto!306)
# Release/v4.3.0-DEV (03-02-2025)
# Release/4.2.7 (09-01-2025)
# Release/4.2.6 (07-01-2025)
# XMLConverterTool/4.2.6.0 (18-12-2024)


## Bug Fixes

- Convert steering pump tech (vecto/vecto!303)
# Release/4.2.6-RC (06-12-2024)


## Features

- Support amdm3 release notes (vecto/vecto!274)

- CodeEU #611: VTP for conventional buses (https://code.europa.eu/vecto/vecto/-/merge_requests/262) (vecto/vecto!262)
- Support Gitlab issue pattern (vecto/vecto!272)

- Verify primary bus VIF hash against Job (vecto/vecto!295)

- Add 3rd amendment XSD definitions (vecto/vecto!291)

- Include Engine into v2.6 (vecto/vecto!297)


## Bug Fixes

- CodeEU #760: Removed wrong exempted techs from XSD (#760) (vecto/vecto!282)
- Homogenize versions across tools (vecto/vecto!270)

- CodeEU #807: Produce same data from ADC loss map (#807) (vecto/vecto!278)
- Authors and readme metadata content (vecto/vecto!277)

- CodeEU #809: Writing engine information in MRF (#809) (vecto/vecto!275)
- Driver model: in case of an APT vehicle where the driving action is Brake... (vecto/vecto!287)

- Conversion of doubles for SI (vecto/vecto!283)

- Avoid wrong upshift and downshift for light SMT vehicles (vecto/vecto!290)

- During a coasting action (look-ahead coasting) a gear hunting occurs in the... (vecto/vecto!289)

- Override DoWriteModalResult for VTP (vecto/vecto!294)

- Add SMT downshift condition - DroppedSpd>DisengSpd (vecto/vecto!293)
# Release/v4.2.5 (02-10-2024)


## Bug Fixes

- Missing Build.props DefineConstants (vecto/vecto!268)

- Version 4th number read from Build.props (vecto/vecto!269)
# Release/v4.2.3 (01-10-2024)


## Bug Fixes

- CodeEU #794: Added missing monitoring report file (#794)
- Stage Directory.Build.props file

- Update changelog script
# Release/v4.2.2.3539-RC (09-09-2024)
# Release/v4.2.1.3469 (01-07-2024)
# Release/v4.0.2.3273 (18-12-2023)
# Release/v4.0.1.3217 (23-10-2023)
# Release/v4.0.0.3161-RC (28-08-2023)
# Release/v4.0.0.3106-RC (04-07-2023)
# Release/v4.0.0.3078-RC (06-06-2023)
# Build/v0.7.10.2996 (16-03-2023)
# Mockup_0.7.9.2906 (05-12-2022)
# Build/v0.7.9.2741 (04-07-2022)
# Build/v0.7.8.2679 (04-05-2022)
# Build/v0.7.5.2451_Multistage_DEA (17-09-2021)
# Release/v0.7.7.2547-DEV (22-12-2021)
# Build/v0.7.6.2451 (17-09-2021)
# Release/v3.3.10.2373 (01-07-2021)
# Build/v0.7.5.2356_xEV (14-06-2021)
# Build/v0.7.4.2351_Multistage (09-06-2021)
# Release/v3.3.6.1898-RC (13-03-2020)
# Release/v3.3.5.1783-RC (19-11-2019)
# Release/v3.3.4.1716 (13-09-2019)
# Release/v3.3.4.1686-RC (14-08-2019)
# Build/v0.7.3.2164 (04-12-2020)
# Build/v0.7.2.2118 (19-10-2020)
# Build/v0.7.1.2108 (09-10-2020)
# Build/v0.7.0.2706 (07-09-2020)




# VECTO v5.0.7 Official Release (14-10-2025)


## Bug Fixes

- Release notes link in GUI dialog (vecto/vecto!480)
- Battery only mode for IHPC (vecto/vecto!481)
- Add System.Data.SqlClient to VECTO.vbproj (vecto/vecto!482)
- Treat 'not applicable' as 'none' in HeatPumpTypeDriverCompartmentType (vecto/vecto!483)
- Correct parsing of fuel cell in interim file (vecto/vecto!484)
- Proper mapping for ADC loss-map in XML (vecto/vecto!485)
- Improved error message for missing battery SoC bounds (vecto/vecto!486)
- VETO main version in dialogs (vecto/vecto!488)
- Corrected XSD dynamic charging types (vecto/vecto!491)
- Introduce new mod data postprocessing for BO-HEV (vecto/vecto!489)
- Copy monitoring data from input (for completed vehicles) (vecto/vecto!494)
- Generic vehicles engineering mode VTP files (vecto/vecto!492)
- Surround post-mortem analysis with try/catch block (vecto/vecto!495)
- Iepc gearshift (vecto/vecto!487)
- Iepc gearshift torque reserve (post MR !487) (vecto/vecto!496)
- Corrected name for Tyre in VIF (vecto/vecto!497)
- Corrected json sample file (vecto/vecto!498)
- Added missing bindings for mod-data post processing (vecto/vecto!499)
- Allow VTP with v2.4 vehicles. (vecto/vecto!503)
- Updated some sample vehicles to v2.7  (vecto/vecto!504)

## Refactor

- Remove redundant code (vecto/vecto!493)



# VECTO v5.0.6-RC (22-09-2025)


## Features

- Update jobs in Generic Vehicles to version v2.7 (vecto/vecto!453)
- Disable v2.4 jobs (vecto/vecto!458)
- CodeEU #1140: Update multistep GUI to work with new XSDs (vecto/vecto!469)
- Multiple axles partial implementation (vecto/vecto!471)


## Bug Fixes

- Fix FCHV unit tests (vecto/vecto!448)
- XMLConversionTool bug fixes, more test cases, refactoring (vecto/vecto!452)
- Bug fixes for FCHV bus (vecto/vecto!454)
- CodeEU #1147: Made FuelCell Minpower, Maxpower optional (vecto/vecto!456)
- Engine-only simulation (vecto/vecto!457)
- EM data in PHEV rundata creation (vecto/vecto!459)
- CodeEU #1164: Lifetime ranges in reports for PEV, HEV-OVC. (vecto/vecto!461)
- Work-around in ranges to make tests succeed (vecto/vecto!462)
- CodeEU #1163: Respect job's battery SoC limits (vecto/vecto!463)
- CodeEU #870, #871, #924: Extend Accelerate condition after xEV Overload (vecto/vecto!466)
- VTP generic vehicles (vecto/vecto!465)
- Extend Accelerate condition after xEV Overload
- Changed v2.6 XSD to allow DeltaCdxA_declared and DeltaTransferredCdxA value: zero (vecto/vecto!472)
- Avoid cyclic refs from !473 (vecto/vecto!475)
- CodeEU #1167: Added WheelEnd info to MRF (vecto/vecto!476)
- Angledrive mod data, and PWheel axlegear efficiency (vecto/vecto!477)
- Disable engineering mode for multiple powertrains (vecto/vecto!478)


## Refactor

- FCHV iterative run strategy  (vecto/vecto!449)
- Update VECTO to NET 8 (vecto/vecto!467)
- Old .NET references (vecto/vecto!468)
- MultistepTool deprecated views (vecto/vecto!470)
- Remove unnecessary usings and nugets (vecto/vecto!473)


# VECTO v5.0.4-DEV (25-08-2025)


## Features

- 3rd amendment reports for buses (vecto/vecto!421)
- V1.0 reports for multiple powertrain lorries (vecto/vecto!395)
- Add Diesel B100 CI fuel (vecto/vecto!399)
- MRF and Monitoring report for multiple-powertrain primary buses (vecto/vecto!410)
- battery only mode for P2 (vecto/vecto!425)
- Run simulation for H2-ICE bus (primary + completed) (vecto/vecto!432)
- FCHV bus simulation, primary & completed (vecto/vecto!433)
- Single-bus mode for FCHV (vecto/vecto!435)
- Enable all v2.7 vehicles (vecto/vecto!436)
- VTP input and formulas for buses and trucks (vecto/vecto!424)


## Bug Fixes

- Exception when getting MaxWindowsSize (vecto/vecto!422)
- For FCHV, APT-S/P gearboxes simulated as APT-N. (vecto/vecto!423)
- Ovc s-hev cs cd (vecto/vecto!420)
- Convert property Type to Architecture in axle powertrains (vecto/vecto!393)
- 3 job types for multiple powertrains (vecto/vecto!394)
- Removed NgTankSystem from Multiple_SHEV primary bus (vecto/vecto!398)
- Remove Retarder component from X4 architectures (vecto/vecto!400)
- Use multiple factory methods for Retarder and Angledrive data providers (vecto/vecto!406)
- Added FCHV missing gearbox bindings (vecto/vecto!427)
- Airdrag element in VIF report. (vecto/vecto!430)
- Do not require SoC limits for HV non-OVC (vecto/vecto!431)
- B100 density to 890 kg/m3 (vecto/vecto!411)
- Run old VIFs with v2.7 Completed vehicles (vecto/vecto!434)
- 1065 vehicle co2 group (vecto/vecto!428)
- DoCoast - add drive condition for overload (vecto/vecto!426)
- Generic retarder and failing tests (vecto/vecto!437)
- IEPC data adaptation (vecto/vecto!438)
- IHPC VECTO run data (vecto/vecto!439)
- FCHV IEPC rundata gearbox creation (vecto/vecto!440)
- Generic vehicles that failed to run (vecto/vecto!441)
- WHRCharger creation (vecto/vecto!442)
- Update wheelEnd sample (vecto/vecto!443)
- FCHV files in engineering mode (vecto/vecto!444)
- Initialize MaxChargingPower from static data if not available in input (vecto/vecto!445)
- FCHV battery and CD and CS runs (vecto/vecto!446)

## Refactor

- Merge refactoring branch (vecto/vecto!419)
- Merging refactorings from SW3 project to (vecto/vecto!396)



# VECTO v5.0.3 Official Release (08-07-2025)


## Bug Fixes

- Avoid em conditioning calculation for non-FCHV (vecto/vecto!416)
- Safe conditioning data lookup (vecto/vecto!417)


# VECTO v5.0.1 Official Release (03-07-2025)


## Bug Fixes

- FCHV engineering run (vecto/vecto!391)

- H2 check in bus job (vecto/vecto!392)

- Auxiliaries REESS connection (vecto/vecto!397)

- Read PowerOutputConsumptionMap as kW (vecto/vecto!401)

- Select pruned missions for FCHV primary bus (vecto/vecto!403)

- Use angledrive in lorries' gearshift data creation (vecto/vecto!404)

- Remove wrong bus angledrive restrictions (vecto/vecto!405)

- Decl GUI error message when FCHV in eng mode (vecto/vecto!407)

- Set NgTankSystem default for primary buses (vecto/vecto!408)

- FCHV F-IEPC simulation runs! (vecto/vecto!409)

- Operational range for group 10 vehicle weights (vecto/vecto!402)

- Ovc fc weighting to correspond to CO2 computation (vecto/vecto!413)

- 3s Buffer compute max EM PLoss for FL OPs (vecto/vecto!412)

- Conditioning power demand for FCHVs (vecto/vecto!414)




# VECTO v5.0.0-RC (05-06-2025)


## Features

- New (v2.7) XSD for vehicles (vecto/vecto!334)

- Partial implementation for new vehicle battery (vecto/vecto!337)

- Readers for v2.7 vehicle XSD, and support for fuel cell vehicles. (vecto/vecto!341)

- Read monitoring data from job (vecto/vecto!345)

- 3rd amendment mrf cif xml schemas (vecto/vecto!340)

- MRF v1.0 vehicle (lorries and FCHV primary buses) writers (vecto/vecto!354)

- CIF v1.0 vehicle part (v2.4 vehicles and v2.7 lorries) (vecto/vecto!355)

- Use monitoring data from job to write report (vecto/vecto!360)

- In motion charging postprocessing (vecto/vecto!344)

- Disable (for RC & official) v27 vehicles except H2-ICE & FCHV lorries (vecto/vecto!375)

- EM-IEPC Thermal Derating - Tq_max and Buffer Mods - Post VECTO-4.3.4 Feed-Back (vecto/vecto!378)

- Forbid AT upshift for reduced dt before brake (vecto/vecto!381)

- Readers for v2.7 buses, improved reader tests. (vecto/vecto!382)

- HEV - Get Best dSOC in vsum (vecto/vecto!383)


## Bug Fixes

- Non-https link in manual (vecto/vecto!339)

- V2.7 reader & XSD (vecto/vecto!342)

- Updated Monitoring Report XSD (FCHV, Multiple powertrains) (vecto/vecto!335)

- Updated XSLT file and hashing code for new vehicles and components. (vecto/vecto!338)

- Lock StoredResults list before accessing it to avoid race condition (vecto/vecto!343)

- V27 vehicle issues (vecto/vecto!346)

- 882 merge artifacts (vecto/vecto!347)

- FCHV angledrive input (vecto/vecto!348)

- Modify schema so that results can be written compatible with results for 2nd amendment: (vecto/vecto!349)

- Restore deleted code in monitoring report (vecto/vecto!350)

- Typo in MRF Inject module (vecto/vecto!351)

- Proper namespace for VIF IEPC sub-element (vecto/vecto!352)

- Mockup tests run successfully (vecto/vecto!353)

- Correcting errors in XML schema (and sample files): no engine output in... (vecto/vecto!356)

- Bugfixes/updates for the Monitoring report and testing via the MockupTests. (vecto/vecto!357)

- Replace U+2013 by regular dashes (vecto/vecto!359)

- Added missing IMC testdata (vecto/vecto!361)

- Retarder compulsory in all MRF vehicle components. (vecto/vecto!362)

- Add further condition to decide which results to write in case the input data is a Multistep bus (vecto/vecto!363)

- Check Articulated in json vehicle (vecto/vecto!365)

- Segment in Bus AirDrag data creation (vecto/vecto!364)

- FCHV pre-run execution (vecto/vecto!366)

- Write ZeroCO2EmissionsRange and HydrogenRange to H2-ICE reports. (vecto/vecto!367)

- Remove wrong angledrive restrictions (vecto/vecto!369)

- Set vectorundata in completed bus results, (vecto/vecto!368)

- Simulate OVC for FCHVs (vecto/vecto!370)

- FCHV H2 range in reports (vecto/vecto!371)

- Architecture in some MRF v1.0 tests (vecto/vecto!372)

- H2 properties check in exempted vehicle input (vecto/vecto!373)

- FCHV input classes inheritance (vecto/vecto!374)

- Standard values enum entry for v2.6 (vecto/vecto!376)

- Add Driving Actions for IEPC gearshift (vecto/vecto!377)

- ReEngage1C tolerance in AT (vecto/vecto!379)

- Take battery limit into account for EM overload - REESS Empty  (vecto/vecto!380)

- Of v2.7, allow only IMC, H2-ICE conventional, FCHV Lorries (vecto/vecto!384)

- Disable reading data from external csv (vecto/vecto!385)

- OVC results (vecto/vecto!387)

- Ignore FCHV pre-run in best deltaSoC calculation (vecto/vecto!388)


## Drop

- MaxChargingPower requirement for OVC in v2.7 (vecto/vecto!358)




