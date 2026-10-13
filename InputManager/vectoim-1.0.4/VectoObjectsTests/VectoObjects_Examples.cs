using System;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoObjectsTests;

#pragma warning disable CS8600  // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602  // Dereference of a possibly null reference.
#pragma warning disable IDE0054 // Use compound assignment

public class VectoObjects_Examples
{
    [Test]
    public void BasicVehicleJobCreation_Example()
    {
        // Create a conventional heavy lorry vehicle job
        var vehicle = new VectoDeclarationJobType_v2_7
        {
            Vehicle = new Vehicle_Conventional_HeavyLorryDeclarationType_v2_7
            {
                Manufacturer = "Example Manufacturer",
                ManufacturerAddress = "Example City",
                Model = "Heavy Lorry Model X",
                VIN = "1234567890ABCDEFG",
                Date = DateTime.UtcNow,
                LegislativeCategory = LegislativeCategoryHeavyLorryDeclarationType_v2_4.N3,
                CorrectedActualMass = 7500,
                TechnicalPermissibleMaximumLadenMass = 40000,

                // Define basic vehicle components
                Components = new Components_Conventional_LorryType_v2_7
                {
                    Engine = new EngineComponentDeclarationType_v2_0
                    {
                        Data = new EngineDataDeclarationType_v1_0
                        {
                            Manufacturer = "Engine Corp",
                            Model = "EcoEngine 2024",
                            Date = DateTime.UtcNow,
                            AppVersion = "1.0.0",
                            Displacement = 12800,
                            IdlingSpeed = 650,
                            RatedSpeed = 1900,
                            RatedPower = 350000, // 350 kW
                            MaxEngineTorque = 2300,
                            FuelType = FuelTypeType_v1_0.NG,
                            id = "engine-1"
                        }
                    },

                    Gearbox = new GearboxComponentDeclarationType_v2_0
                    {
                        Data = new GearboxDataDeclarationType_v1_0
                        {
                            Manufacturer = "Transmission Ltd",
                            Model = "AutoGear Pro",
                            Date = DateTime.UtcNow,
                            AppVersion = "1.0.0",
                            TransmissionType = GearboxTransmissionTypeType_v1_0.AMT,
                            id = "gearbox-1"
                        }
                    }
                }
            }
        };

        // Serialize to XML file
        VectoXml.SerializeJob("my_vehicle.xml", vehicle);

        // Test deserialization and reading a few random properties we set above.
        using var fs = File.OpenRead("my_vehicle.xml");
        VectoXml.TryGetSerializer(fs, out var serializer, out var rootType, out var error);
        var job2 = (IVehicleJobDeclaration)serializer.Deserialize(fs);
        var vehicle2 = job2.Vehicle as Vehicle_Conventional_HeavyLorryDeclarationType_v2_7;

        Assert.That(
            ((GearboxDataDeclarationType_v1_0)vehicle2.Components.Gearbox.Data).Model,
            Is.EqualTo("AutoGear Pro"));

        Assert.That(vehicle2.LegislativeCategory,
            Is.EqualTo(LegislativeCategoryHeavyLorryDeclarationType_v2_4.N3));
        
        Assert.That(vehicle2.TechnicalPermissibleMaximumLadenMass, Is.EqualTo(40000));

        Assert.That(((EngineDataDeclarationType_v1_0)vehicle2.Components.Engine.Data).RatedPower,
            Is.EqualTo(350000));
    }

    [Test]
    public void LoadAndModifyExistingVehicle_Example1()
    {
        // Load existing XML file
        using var fileStream = File.OpenRead("Conventional_HeavyLorry.xml");
        // Peek at the file and get the appropriate serializer
        VectoXml.TryGetSerializer(fileStream, out var serializer, out var rootType, out var error);
        // Deserialize the vehicle job
        var vehicleJob = (IVehicleJobDeclaration)serializer.Deserialize(fileStream);

        var vehicle = vehicleJob.Vehicle as Vehicle_Conventional_HeavyLorryDeclarationType_v2_7;

        // BASIC VEHICLE INFORMATION UPDATES
        vehicle.Model = vehicle.Model + " Performance Edition";
        vehicle.Date = DateTime.UtcNow;
        vehicle.VehicleTypeApprovalNumber = "PERF-2024-001";

        // Update vehicle configuration
        vehicle.ChassisConfiguration = ChassisConfigurationHeavyLorryDeclarationType_v2_4.Tractor;
        vehicle.AxleConfiguration = AxleConfigurationHeavyLorryDeclarationType_v2_4.Item4x2;

        // Weight optimization - reduce unladen weight by 200kg
        vehicle.CorrectedActualMass = Math.Max(6795, vehicle.CorrectedActualMass - 200);

        // ENGINE PERFORMANCE UPGRADES
        var engine = vehicle.Components?.Engine?.Data as EngineDataDeclarationType_v1_0;

        // Performance engine upgrade
        engine.Model = engine.Model.Replace("Generic", "Performance") + " Turbo";
        engine.Date = DateTime.UtcNow;

        // Increase power and torque
        engine.RatedPower = 380000; // Upgrade from 325kW to 380kW
        engine.MaxEngineTorque = 2400; // Increase from 2134 to 2400 Nm
        engine.RatedSpeed = 1850; // Slightly lower for better torque curve
        engine.IdlingSpeed = 580; // Lower idle for fuel efficiency

        // Update fuel consumption factors for improved efficiency
        engine.WHTCUrban = 1.0350m; // Slight improvement in urban cycle
        engine.WHTCRural = 1.0150m; // Better rural efficiency
        engine.WHTCMotorway = 0.9980m; // Optimized for highway driving
        engine.BFColdHot = 1.0000m; // Improved cold-hot factor

        // TRANSMISSION OPTIMIZATION
        var gearbox = vehicle.Components?.Gearbox?.Data as GearboxDataDeclarationType_v2_0;

        gearbox.Model = gearbox.Model.Replace("Generic", "Heavy Duty") + " Eco";
        gearbox.Date = DateTime.UtcNow;

        // Optimize gear ratios for better fuel economy
        if (gearbox.Gears is GearsDeclarationType_v2_0 gears)
        {
            // Adjust top gear ratio for highway efficiency
            var topGear = gears.Gear.LastOrDefault();
            if (topGear != null && topGear.Ratio > 1.1m)
            {
                topGear.Ratio = 0.920m; // Overdrive gear for highway efficiency
            }

            // Add an additional gear for better progression
            if (gears.Gear.Count < 16)
            {
                gears.Gear.Add(new GearDeclarationType_v2_0
                {
                    number = gears.Gear.Count + 1,
                    Ratio = 0.780m // Additional overdrive
                });
            }
        }

        // AXLE OPTIMIZATION
        if (vehicle.Components?.Axlegear?.Data is AxlegearDataDeclarationType_v1_0 axlegear)
        {
            axlegear.Model = axlegear.Model + " Efficiency";
            axlegear.Date = DateTime.UtcNow;

            // Optimize axle ratio for fuel economy
            axlegear.Ratio = 2.490m; // Reduce from 2.640 for highway efficiency
        }

        // TIRE UPGRADES
        var axleWheels = vehicle.Components?.AxleWheels?.Data as AxleWheelsDataDeclarationType_v2_7;

        foreach (var axle in axleWheels.Axles)
        {
            if (axle.Tyre?.Data is TyreDataDeclarationType_v1_0 tyre)
            {
                tyre.Model = tyre.Model + " LowRR Pro";
                tyre.Date = DateTime.UtcNow;

                // Ultra-low rolling resistance tires
                if (axle.AxleType.ToString().Contains("NonDriven"))
                {
                    // Steer axle - prioritize low rolling resistance
                    tyre.RRCDeclared = 0.0029m; // Improved from 0.0034
                }
                else
                {
                    // Drive axle - balance efficiency with traction
                    tyre.RRCDeclared = 0.0035m; // Improved from 0.0040
                }
            }
        }

        // AERODYNAMIC IMPROVEMENTS
        var airDrag = vehicle.Components?.AirDrag?.Data as AirDragDataDeclarationType_v2_0;

        airDrag.Model = airDrag.Model + " Aero Package";
        airDrag.Date = DateTime.UtcNow;

        // Aerodynamic improvements: side skirts, roof fairing, etc.
        airDrag.CdxA_0 = 4.15m; // Reduce from 4.31 (4% improvement)
        airDrag.TransferredCdxA = 4.15m;
        airDrag.DeclaredCdxA = 4.15m;


        // RETARDER OPTIMIZATION
        var retarder = vehicle.Components?.Retarder?.Data as RetarderDataDeclarationType_v2_0;

        retarder.Model = retarder.Model + " Efficient";
        retarder.Date = DateTime.UtcNow;

        // Reduce retarder losses when not in use
        foreach (var entry in retarder.RetarderLossMap)
        {
            // Reduce standby losses by 10%
            entry.torqueLoss = Math.Max(8.0m, entry.torqueLoss * 0.90m);
        }

        // ADVANCED DRIVER ASSISTANCE SYSTEMS
        vehicle.ADAS ??= new ADAS_Conventional_Type_v2_7();

        // Enable advanced fuel-saving features
        vehicle.ADAS.EngineStopStart = true;
        vehicle.ADAS.EcoRollWithEngineStop = true;
        vehicle.ADAS.EcoRollWithoutEngineStop = false; // Prefer engine stop for max efficiency
        vehicle.ADAS.PredictiveCruiseControl = PredictiveCruiseControlType_v2_1.none;

        var aux = vehicle.Components.Auxiliaries.Data;

        // ADDITIONAL AUXILIARY SYSTEM
        aux.Fan.Technology = AuxFanTechnologyLorryType_v2_4.Crankshaft_mounted_OnSlashoff_clutch;
        aux.ElectricSystem.Technology = AuxESTechnologyLorryType_v2_4.Standard_technology;
        aux.PneumaticSystem.Technology = AuxPSTechnologyLorryType_v2_7.Vacuum_pump;

        vehicle.ZeroEmissionVehicle = false;
        vehicle.VocationalVehicle = true;
        vehicle.SleeperCab = true;

        // TORQUE LIMITS
        foreach (var limit in vehicle.TorqueLimits)
        {
            // Increase torque limits
            limit.maxTorque = Math.Min(2400, limit.maxTorque + 200);
        }

        // Save the modified vehicle
        VectoXml.SerializeJob("Modified_Conventional_HeavyLorry.xml", vehicleJob);

        // Test deserialization and reading a few random properties we set above.
        using var fs = File.OpenRead("Modified_Conventional_HeavyLorry.xml");
        VectoXml.TryGetSerializer(fs, out var testSerializer, out var testRootType, out var testError);
        var testJob = (IVehicleJobDeclaration)testSerializer.Deserialize(fs);
        var testVehicle = testJob.Vehicle as Vehicle_Conventional_HeavyLorryDeclarationType_v2_7;

        // Assertions for modified vehicle properties
        Assert.That(testVehicle.ChassisConfiguration,
            Is.EqualTo(ChassisConfigurationHeavyLorryDeclarationType_v2_4.Tractor));
        Assert.That(testVehicle.VocationalVehicle, Is.True);
        Assert.That(testVehicle.SleeperCab, Is.True);
    }

    [Test]
    public void LoadAndModifyExistingVehicle_Example2()
    {
        // Load existing XML file
        using var fileStream = File.OpenRead("Multiple_PEV_E2_IEPC_PrimaryBus.xml");
        // Peek at the file and get the appropriate serializer
        VectoXml.TryGetSerializer(fileStream, out var serializer, out var rootType, out var error);
        // Deserialize the vehicle job
        var vehicleJob = (IVehicleJobDeclaration)serializer.Deserialize(fileStream);

        var vehicle = vehicleJob.Vehicle as Vehicle_Multiple_PEV_PrimaryBusDeclarationType_v2_7;

        // BASIC VEHICLE INFORMATION UPDATES
        vehicle.Model = vehicle.Model + " Performance Edition";
        vehicle.Date = DateTime.UtcNow;

        vehicle.Articulated = false; // Rigid bus for urban efficiency

        // Weight optimization for electric bus
        vehicle.TechnicalPermissibleMaximumLadenMass =
            Math.Min(30000, vehicle.TechnicalPermissibleMaximumLadenMass + 2000);

        foreach (var battery in vehicle.Components.ElectricEnergyStorage.Battery)
        {
            var batteryData = battery.REESS?.Data as BatterySystemDataType_v2_3;

            batteryData.Model = batteryData.Model + " 2";
            batteryData.Date = DateTime.UtcNow;

            batteryData.RatedCapacity = 450.00m;
            batteryData.BatteryType = BatteryTypeType_v2_3.HPBS;

            // Improved thermal management
            batteryData.TestingTemperature = 23;

            // Enhanced voltage characteristics
            foreach (var ocvEntry in batteryData.OCV)
            {
                if (ocvEntry.SoC == 0)
                {
                    ocvEntry.OCV = 680.00m;
                }
                else if (ocvEntry.SoC == 100)
                {
                    ocvEntry.OCV = 760.50m; // Higher peak voltage
                }

                foreach (var resistance in batteryData.InternalResistance)
                {
                    resistance.R_2 = resistance.R_2 * 0.85m;
                    resistance.R_10 = resistance.R_10 * 0.85m;
                    resistance.R_20 = resistance.R_20 * 0.85m;
                }

                foreach (var currentLimit in batteryData.CurrentLimits)
                {
                    currentLimit.maxChargingCurrent = 750.00m;
                    currentLimit.maxDischargingCurrent = 650.00m;
                }
            }

            // First Powertrain is an Electric Machine with DC-DC Converter
            var pt1 = vehicle.Components.Powertrain[0] as xEV_EM2_Powertrain_Type_v2_7;
            var adc = pt1.ElectricMachine.ElectricMachineSystem.Data as ElectricMachineSystemMeasuredDataDeclarationType_v2_3;
            adc.DcDcConverterIncluded = true;

            // Second Powertrain is an IEPC system
            var pt2 = vehicle.Components.Powertrain[1] as xEV_IEPC_Powertrain_Type_v2_7;
            var iepc = pt2.IEPC.Data as IEPCMeasuredDataDeclarationType_v2_6;
            iepc.DisengagementClutchValue = false;
        }

        vehicle.OVC = true;
        vehicle.LegislativeCategory = LegislativeCategoryPrimaryBusDeclarationType_v2_4.M3; // Public transport

        // Save the modified vehicle
        VectoXml.SerializeJob("Modified_Multiple_PEV_E2_IEPC_PrimaryBus.xml", vehicleJob);

        // Test deserialization and reading a few random properties we set above.
        using var fs = File.OpenRead("Modified_Multiple_PEV_E2_IEPC_PrimaryBus.xml");
        VectoXml.TryGetSerializer(fs, out var testSerializer, out var testRootType, out var testError);
        var testJob = (IVehicleJobDeclaration)testSerializer.Deserialize(fs);
        var testVehicle = testJob.Vehicle as Vehicle_Multiple_PEV_PrimaryBusDeclarationType_v2_7;

        Assert.That(testVehicle.LegislativeCategory,
            Is.EqualTo(LegislativeCategoryPrimaryBusDeclarationType_v2_4.M3));
        Assert.That(testVehicle.OVC, Is.True);
        Assert.That(testVehicle.Articulated, Is.False);
    }
}