using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class DataSet
{

    public string[] xLabels;
    public string[] yLabels;
    public float[,] data;
    public float minValue;
    public float maxValue;
    public int orientation;
    public int direction;

    public DataSet(float[,] data, string[] xLabels, string[] yLabels, float minValue, float maxValue, int orientation, int direction)
	{
		this.data = data;
		this.xLabels = xLabels;
		this.yLabels = yLabels;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.orientation = orientation;
        this.direction = direction;
    }
}

[Serializable]
public class Data
{
    public List<Building> buildings;
    public List<Room> rooms;
    public List<Device> devices;
    public float[,] minValues;
    public float[,] maxValues;
    public Data(List<Building> buildings, List<Room> rooms, List<Device> devices, float[,] minValues, float [,] maxValues)
    {
        this.buildings = buildings;
        this.rooms = rooms;
        this.devices = devices;
        this.minValues = minValues;
        this.maxValues = maxValues;
    }
}

[Serializable]
public class Building
{
    public string name;
    public Dictionary<DateTime, float> months;
    public Dictionary<DateTime, float> days;
    public Dictionary<DateTime, float> hours;
    public List<Room> rooms;
    public Building(string name)
	{
        this.name = name;
        months = new Dictionary<DateTime, float>();
        days = new Dictionary<DateTime, float>();
        hours = new Dictionary<DateTime, float>();
        rooms = new List<Room>();
    }
    public bool isCoherent()
    {
        foreach (var month in months)
        {
            DateTime currentDate = month.Key;
            float monthValue = month.Value;
            int nbDays = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            float daySum = 0f;
            for (var i = 0; i < nbDays; i++)
            {
                DateTime dayDate = currentDate.AddDays(i);
                float dayValue = days[dayDate];
                daySum += dayValue;

                int nbHours = 24;
                float hourSum = 0f;
                for (var j = 0; j < nbHours; j++)
                {
                    DateTime hourDate = dayDate.AddHours(j);
                    hourSum += hours[hourDate];
                }
                Assert.AreApproximatelyEqual(Mathf.Round(dayValue), Mathf.Round(hourSum), tolerance: dayValue * 0.01f, "Building > dayValue != hourSum"); ;
            }
            Assert.AreApproximatelyEqual(Mathf.Round(monthValue), Mathf.Round(daySum), tolerance: monthValue * 0.01f,  "Building > MonthValue != daySum");
        }
        return true;
    }
}
[Serializable]
public class Room
{
    public string name;
    public Dictionary<DateTime, float> months;
    public Dictionary<DateTime, float> days;
    public Dictionary<DateTime, float> hours;
    public List<Device> devices;
    public Room(string name)
    {
        this.name = name;
        months = new Dictionary<DateTime, float>();
        days = new Dictionary<DateTime, float>();
        hours = new Dictionary<DateTime, float>();
        devices = new List<Device>();
    }
    public bool isCoherent()
    {
        foreach (var month in months)
        {
            DateTime currentDate = month.Key;
            float monthValue = month.Value;
            int nbDays = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            float daySum = 0f;
            for (var i = 0; i < nbDays; i++)
            {
                DateTime dayDate = currentDate.AddDays(i);
                float dayValue = days[dayDate];
                daySum += dayValue;

                int nbHours = 24;
                float hourSum = 0f;
                for (var j = 0; j < nbHours; j++)
                {
                    DateTime hourDate = dayDate.AddHours(j);
                    hourSum += hours[hourDate];
                }
                Assert.AreApproximatelyEqual(Mathf.Round(dayValue), Mathf.Round(hourSum), tolerance: dayValue * 0.01f, "Room > dayValue != hourSum"); ;
            }
            Assert.AreApproximatelyEqual(Mathf.Round(monthValue), Mathf.Round(daySum), tolerance: monthValue * 0.01f, "Room > MonthValue != daySum");
        }
        return true;
    }
}
[Serializable]
public class Device
{
    public string name;
    public Dictionary<DateTime, float> months;
    public Dictionary<DateTime, float> days;
    public Dictionary<DateTime, float> hours;
    public Device(string name)
    {
        this.name = name;
        months = new Dictionary<DateTime, float>();
        days = new Dictionary<DateTime, float>();
        hours = new Dictionary<DateTime, float>();
    }
    public bool isCoherent()
    {
        foreach (var month in months)
        {
            DateTime currentDate = month.Key;
            float monthValue = month.Value;
            int nbDays = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            float daySum = 0f;
            for (var i = 0; i < nbDays; i++)
            {
                DateTime dayDate = currentDate.AddDays(i);
                float dayValue = days[dayDate];
                daySum += dayValue;

                int nbHours = 24;
                float hourSum = 0f;
                for (var j = 0; j < nbHours; j++)
                {
                    DateTime hourDate = dayDate.AddHours(j);
                    hourSum += hours[hourDate];
                }
                Assert.AreApproximatelyEqual(Mathf.Round(dayValue), Mathf.Round(hourSum), tolerance: dayValue * 0.01f, "Device > dayValue != hourSum"); ;
            }
            Assert.AreApproximatelyEqual(Mathf.Round(monthValue), Mathf.Round(daySum), tolerance: monthValue * 0.01f, "Device > MonthValue != daySum");
        }
        return true;
    }
}



public class DataPhysModel
{
    // NAVIGATION
    public const int X_AXIS_IN = 1;
    public const int X_AXIS_OUT = -1;
    public const int Y_AXIS_IN = 1;
    public const int Y_AXIS_OUT = -1;
    public const int X_AXIS_LEFT = -1;
    public const int X_AXIS_IDLE = 0;
    public const int X_AXIS_RIGHT = 1;
    public const int Y_AXIS_DOWN = -1;
    public const int Y_AXIS_IDLE = 0;
    public const int Y_AXIS_UP = 1;

    public const int Z_AXIS_0 = 0;
    public const int Z_AXIS_90 = 1;
    public const int Z_AXIS_180 = 2;
    public const int Z_AXIS_270 = 3;

    public const int Z_AXIS_CW = 1;
    public const int Z_AXIS_IDLE = 0;
    public const int Z_AXIS_CCW = -1;

    public const string DATETIME_FORMAT_MONTH = "MMM yyyy";
    public const string DATETIME_FORMAT_DAY = "MMM yyyy\nddd d";
    public const string DATETIME_FORMAT_HOUR = "MMM yyyy\nddd d\nHH:mm";


    // DATA
    private const int TIME_SCALE_MONTH = 0;
    private const int TIME_SCALE_DAY = 1;
    private const int TIME_SCALE_HOUR = 2;
    private  int[] TIME_SCALES = new int[] { TIME_SCALE_MONTH, TIME_SCALE_DAY, TIME_SCALE_HOUR};
    private const int SPACE_SCALE_BUILDING = 0;
    private const int SPACE_SCALE_ROOM = 1;
    private const int SPACE_SCALE_DEVICE = 2;
    private int[] SPACE_SCALES = new int[] { SPACE_SCALE_BUILDING, SPACE_SCALE_ROOM, SPACE_SCALE_DEVICE };
    private float[,] MIN_DATA;
    private float[,] MAX_DATA;
    private DateTime startDate = new DateTime(2021, 1, 1);
    private string[] buildingNames = new string[] { "ESTIA1", "ESTIA2", "ESTIA3", "ESTIA4", "ESTIA5", "ESTIA6", "ESTIA7"};
    private string[] roomNames = new string[] { "ROOM1", "ROOM2", "ROOM3", "ROOM4", "ROOM5", "ROOM6", "ROOM7"};
    private string[] deviceNames = new string[] { "DEVICE1", "DEVICE2", "DEVICE3", "DEVICE4", "DEVICE5", "DEVICE6", "DEVICE7"};
    
    private List<Building> buildings;
    private List<Room> rooms;
    private List<Device> devices;

    private List<DateTime> months;
    private List<DateTime> days;
    private List<DateTime> hours;

    private Data data;
    private string dataFile = Application.persistentDataPath + "/data.dat";
    private int currentTimeScale = TIME_SCALE_MONTH;
    private int currentTimePos = 0;
    private int currentTimeSize = 0;
    private int currentTimeLength = 0;
    private int currentSpaceScale = SPACE_SCALE_BUILDING;
    private int currentSpacePos = 0;
    private int currentSpaceSize = 0;
    private int currentSpaceLength = 0;
    private int currentTimeSpaceOrientation = Z_AXIS_0;
    private int currentTimeSpaceDirection = Z_AXIS_CW;

    //Dictionary<string, List<float>> toYAxis  = new Dictionary<string, List<float>>();
    // Start is called before the first frame update
    public DataPhysModel(int nbRows, int nbColumns)
    {
        currentSpaceLength = nbRows;
        currentTimeLength = nbColumns;
        // init data structure
        MIN_DATA = new float[TIME_SCALES.Length, SPACE_SCALES.Length];
        MAX_DATA = new float[TIME_SCALES.Length, SPACE_SCALES.Length];

        months = new List<DateTime>();
        days = new List<DateTime>();
        hours = new List<DateTime>();
        DateTime currentDate = startDate;
        int nbMonths = 12;
        for (int m = 0; m < nbMonths; m++)
        {
            DateTime monthDate = currentDate.AddMonths(m);
            months.Add(monthDate);
            int nbDays = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
            for (int d = 0; d < nbDays; d++)
            {
                DateTime dayDate = monthDate.AddDays(d);
                days.Add(dayDate);
                int nbHours = 24;
                for (int h = 0; h < nbHours; h++)
                {
                    DateTime hourDate = dayDate.AddHours(h);
                    hours.Add(hourDate);
                }
            }
        }

        buildings = new List<Building>();
        rooms = new List<Room>();
        devices = new List<Device>();

        // Check if Data file exists
       /* if (File.Exists(dataFile))
        {
            Debug.Log("Loading data to file...");
            LoadDataFromFile();
            MIN_DATA = data.minValues;
            MAX_DATA = data.maxValues;
            buildings = data.buildings;
            rooms = data.rooms;
            devices = data.devices;

        } else
        {*/
            foreach (var buildingName in buildingNames)
            {
                Building building = new Building(buildingName);
                buildings.Add(building);
                foreach (var roomName in roomNames)
                {
                    Room room = new Room(buildingName + "\n" + roomName);
                    building.rooms.Add(room);
                    rooms.Add(room);
                    foreach (var deviceName in deviceNames)
                    {
                        Device device = new Device(buildingName + "\n" + roomName + "\n" + deviceName);
                        room.devices.Add(device);
                        devices.Add(device);
                    }
                }
        }
            Debug.Log("Filling devices...");
            FillDevices();
            Debug.Log("Filling rooms...");
            FillRooms();
            Debug.Log("Filling buildings...");
            FillBuildings();
            /*Debug.Log("Checking coherence...");
            CheckCoherence();*/
            Debug.Log("Saving data to file...");
            data = new Data(buildings, rooms, devices, MIN_DATA, MAX_DATA);
            //SaveDataToFile();
       // }
        Debug.Log("Init scale...");
        setCurrentScale(currentTimeScale, currentSpaceScale);
    }
    public void SaveDataToFile()
    {
        FileStream file;

        if (File.Exists(dataFile)) file = File.OpenWrite(dataFile);
        else file = File.Create(dataFile);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Data saved to file: " + dataFile);
    }

    public void LoadDataFromFile()
    {
        FileStream file;

        if (File.Exists(dataFile)) file = File.OpenRead(dataFile);
        else
        {
            Debug.LogError("File not found");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        data = (Data)bf.Deserialize(file);
        file.Close();
        Debug.Log("Data loaded from file: " + dataFile);
    }

    private void CheckCoherence()
	{
        foreach (Building building in buildings) building.isCoherent();
        foreach (Room room in rooms) room.isCoherent();
        foreach (Device device in devices) device.isCoherent();
    }
    private void FillDevices()
    {
        float[] randoms = generateShuffledDistribution(devices.Count * hours.Count);

        float minMonths = float.PositiveInfinity;
        float maxMonths = float.NegativeInfinity;
        float minDays = float.PositiveInfinity;
        float maxDays = float.NegativeInfinity;
        float minHours = float.PositiveInfinity;
        float maxHours = float.NegativeInfinity;

        for(int i = 0; i < devices.Count; i++)
        {
            Device device = devices[i];

            // walk hours, days and months
            DateTime currentDate = startDate;
            int nbMonths = 12;
            for (int m = 0; m < nbMonths; m++)
            {
                DateTime monthDate = currentDate.AddMonths(m);
                int nbDays = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
                float sumDays = 0f;
                for (int d = 0; d < nbDays; d++)
                {
                    DateTime dayDate = monthDate.AddDays(d);
                    int nbHours = 24;
                    float sumHours = 0f;
                    for (int h = 0; h < nbHours; h++)
                    {
                        DateTime hourDate = dayDate.AddHours(h);
                        float hourVal = randoms[i * ((dayDate.DayOfYear - 1) * nbHours + h)];
                        device.hours.Add(hourDate, hourVal);
                        sumHours += hourVal;
                        if (hourVal > maxHours) maxHours = hourVal;
                        if (hourVal < minHours) minHours = hourVal;
                    }
                    device.days.Add(dayDate, sumHours);
                    sumDays += sumHours;
                    if (sumHours > maxDays) maxDays = sumHours;
                    if (sumHours < minDays) minDays = sumHours;
                }
                device.months.Add(monthDate, sumDays);
                if (sumDays > maxMonths) maxMonths = sumDays;
                if (sumDays < minMonths) minMonths = sumDays;
            }
        }

        MIN_DATA[TIME_SCALE_HOUR, SPACE_SCALE_DEVICE] = minHours;
        MAX_DATA[TIME_SCALE_HOUR, SPACE_SCALE_DEVICE] = maxHours;

        MIN_DATA[TIME_SCALE_DAY, SPACE_SCALE_DEVICE] = minDays;
        MAX_DATA[TIME_SCALE_DAY, SPACE_SCALE_DEVICE] = maxDays;

        MIN_DATA[TIME_SCALE_MONTH, SPACE_SCALE_DEVICE] = minMonths;
        MAX_DATA[TIME_SCALE_MONTH, SPACE_SCALE_DEVICE] = maxMonths;
        randoms = null;
    }
    private void FillRooms()
    {
        float minMonths = float.PositiveInfinity;
        float maxMonths = float.NegativeInfinity;
        float minDays = float.PositiveInfinity;
        float maxDays = float.NegativeInfinity;
        float minHours = float.PositiveInfinity;
        float maxHours = float.NegativeInfinity;
        foreach (Room room in rooms)
        {
            // walk hours, days and months
            DateTime currentDate = startDate;

            // Fill room month by the sum of devices' month
            int nbMonths = 12;
            for (int m = 0; m < nbMonths; m++)
            {
                DateTime monthDate = currentDate.AddMonths(m);
                float sumDevicesMonth = 0f;
                foreach (Device device in room.devices) sumDevicesMonth += device.months[monthDate];
                room.months.Add(monthDate, sumDevicesMonth);
                if (sumDevicesMonth > maxMonths) maxMonths = sumDevicesMonth;
                if (sumDevicesMonth < minMonths) minMonths = sumDevicesMonth;

                // Fill room day by the sum of devices' day
                int nbDays = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
                for (int d = 0; d < nbDays; d++)
                {
                    DateTime dayDate = monthDate.AddDays(d);
                    float sumDevicesDay = 0f;
                    foreach (Device device in room.devices) sumDevicesDay += device.days[dayDate];
                    room.days.Add(dayDate, sumDevicesDay);
                    if (sumDevicesDay > maxDays) maxDays = sumDevicesDay;
                    if (sumDevicesDay < minDays) minDays = sumDevicesDay;

                    // Fill room hour by the sum of devices' hour
                    int nbHours = 24;
                    for (int h = 0; h < nbHours; h++)
                    {
                        DateTime hourDate = dayDate.AddHours(h);
                        float sumDevicesHour = 0f;
                        foreach (Device device in room.devices) sumDevicesHour += device.hours[hourDate];
                        room.hours.Add(hourDate, sumDevicesHour);
                        if (sumDevicesHour > maxHours) maxHours = sumDevicesHour;
                        if (sumDevicesHour < minHours) minHours = sumDevicesHour;
                    }
                }
            }
        }

        MIN_DATA[TIME_SCALE_HOUR, SPACE_SCALE_ROOM] = minHours;
        MAX_DATA[TIME_SCALE_HOUR, SPACE_SCALE_ROOM] = maxHours;

        MIN_DATA[TIME_SCALE_DAY, SPACE_SCALE_ROOM] = minDays;
        MAX_DATA[TIME_SCALE_DAY, SPACE_SCALE_ROOM] = maxDays;

        MIN_DATA[TIME_SCALE_MONTH, SPACE_SCALE_ROOM] = minMonths;
        MAX_DATA[TIME_SCALE_MONTH, SPACE_SCALE_ROOM] = maxMonths;
    }
    private void FillBuildings()
    {
        float minMonths = float.PositiveInfinity;
        float maxMonths = float.NegativeInfinity;
        float minDays = float.PositiveInfinity;
        float maxDays = float.NegativeInfinity;
        float minHours = float.PositiveInfinity;
        float maxHours = float.NegativeInfinity;
        foreach (Building building in buildings)
        {
            // walk hours, days and months
            DateTime currentDate = startDate;

            // Fill room month by the sum of devices' month
            int nbMonths = 12;
            for (int m = 0; m < nbMonths; m++)
            {
                DateTime monthDate = currentDate.AddMonths(m);
                float sumRoomsMonth = 0f;
                foreach (Room room in building.rooms) sumRoomsMonth += room.months[monthDate];
                building.months.Add(monthDate, sumRoomsMonth);
                if (sumRoomsMonth > maxMonths) maxMonths = sumRoomsMonth;
                if (sumRoomsMonth < minMonths) minMonths = sumRoomsMonth;


                // Fill room day by the sum of devices' day
                int nbDays = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
                for (int d = 0; d < nbDays; d++)
                {
                    DateTime dayDate = monthDate.AddDays(d);
                    float sumRoomsDay = 0f;
                    foreach (Room room in building.rooms) sumRoomsDay += room.days[dayDate];
                    building.days.Add(dayDate, sumRoomsDay);
                    if (sumRoomsDay > maxDays) maxDays = sumRoomsDay;
                    if (sumRoomsDay < minDays) minDays = sumRoomsDay;

                    // Fill room hour by the sum of devices' hour
                    int nbHours = 24;
                    for (int h = 0; h < nbHours; h++)
                    {
                        DateTime hourDate = dayDate.AddHours(h);
                        float sumRoomsHour = 0f;
                        foreach (Room room in building.rooms) sumRoomsHour += room.hours[hourDate];
                        building.hours.Add(hourDate, sumRoomsHour);
                        if (sumRoomsHour > maxHours) maxHours = sumRoomsHour;
                        if (sumRoomsHour < minHours) minHours = sumRoomsHour;
                    }
                }
            }
        }

        MIN_DATA[TIME_SCALE_HOUR, SPACE_SCALE_BUILDING] = minHours;
        MAX_DATA[TIME_SCALE_HOUR, SPACE_SCALE_BUILDING] = maxHours;

        MIN_DATA[TIME_SCALE_DAY, SPACE_SCALE_BUILDING] = minDays;
        MAX_DATA[TIME_SCALE_DAY, SPACE_SCALE_BUILDING] = maxDays;

        MIN_DATA[TIME_SCALE_MONTH, SPACE_SCALE_BUILDING] = minMonths;
        MAX_DATA[TIME_SCALE_MONTH, SPACE_SCALE_BUILDING] = maxMonths;
    }
    private DataSet FetchDataSet(int timeScale, int _timeStart, int _timeLength, int _timeSize, int spaceScale, int _spaceStart, int _spaceLength, int _spaceSize)
    {
        float minValue = MIN_DATA[timeScale, spaceScale];
        float maxValue = MAX_DATA[timeScale, spaceScale];
        int timeLength = _timeLength;
        int spaceLength = _spaceLength;
        int timeStart = _timeStart;
        int spaceStart = _spaceStart;
        int timeSize = _timeSize;
        int spaceSize = _spaceSize;
        switch (currentTimeSpaceOrientation)
        {
            case Z_AXIS_0:
                timeStart = (timeStart + timeLength > timeSize) ? timeSize - timeLength : timeStart;
                spaceStart = (spaceStart + spaceLength > spaceSize) ? spaceSize - spaceLength : spaceStart;
                float[,]  data = new float[timeLength, spaceLength];
                string[]  xLabels = new string[timeLength];
                string[]  yLabels = new string[spaceLength];
                // string[] timeIds = timeStart.Split('/');
                // string[] spaceIds = timeStart.Split('/');
                switch (timeScale)
                {
                    case TIME_SCALE_MONTH:
                        for (int t = 0; t < timeLength; t++) xLabels[t] = months[timeStart + t].ToString(DATETIME_FORMAT_MONTH);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = buildings[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                    {
                                        data[t, s] = buildings[spaceStart + s].months[months[timeStart + t]];
                                    }
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = rooms[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = rooms[spaceStart + s].months[months[timeStart + t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = devices[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = devices[spaceStart + s].months[months[timeStart + t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_DAY:
                        for (int t = 0; t < timeLength; t++) xLabels[t] = days[timeStart + t].ToString(DATETIME_FORMAT_DAY);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = buildings[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = buildings[spaceStart + s].days[days[timeStart + t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = rooms[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = rooms[spaceStart + s].days[days[timeStart + t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = devices[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = devices[spaceStart + s].days[days[timeStart + t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_HOUR:
                        for (int t = 0; t < timeLength; t++) xLabels[t] = hours[timeStart + t].ToString(DATETIME_FORMAT_HOUR);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = buildings[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = buildings[spaceStart + s].hours[hours[timeStart + t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = rooms[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = rooms[spaceStart + s].hours[hours[timeStart + t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = devices[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = devices[spaceStart + s].hours[hours[timeStart + t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                return new DataSet(data, xLabels, yLabels, minValue, maxValue,currentTimeSpaceOrientation, currentTimeSpaceDirection);
           
            case Z_AXIS_90:
                timeLength = _spaceLength;
                spaceLength = _timeLength;
                timeStart = (timeStart + timeLength > timeSize) ? timeSize - timeLength : timeStart;
                spaceStart = (spaceStart + spaceLength > spaceSize) ? spaceSize - spaceLength : spaceStart;
                data = new float[spaceLength, timeLength];
                xLabels = new string[spaceLength];
                yLabels = new string[timeLength];

                // string[] timeIds = timeStart.Split('/');
                // string[] spaceIds = timeStart.Split('/');
                switch (timeScale)
                {
                    case TIME_SCALE_MONTH:
                        for (int t = 0; t < timeLength; t++) yLabels[t] = months[timeStart + t].ToString(DATETIME_FORMAT_MONTH);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                               /* Debug.Log("Z_AXIS_90");
                                Debug.Log("xLabels.Length: " + xLabels.Length);
                                Debug.Log("spaceLength: " + spaceLength);
                                Debug.Log("(spaceStart + spaceLength - 1): " + (spaceStart + spaceLength - 1));
                                Debug.Log("buildings.Count: " + buildings.Count);*/
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = buildings[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                    {
                                        data[s, t] = buildings[(spaceStart + spaceLength - 1) - s].months[months[timeStart + t]];
                                    }
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = rooms[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = rooms[(spaceStart + spaceLength - 1) - s].months[months[timeStart + t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = devices[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = devices[(spaceStart + spaceLength - 1) - s].months[months[timeStart + t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_DAY:
                        for (int t = 0; t < timeLength; t++) yLabels[t] = days[timeStart + t].ToString(DATETIME_FORMAT_DAY);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = buildings[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = buildings[(spaceStart + spaceLength - 1) - s].days[days[timeStart + t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = rooms[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = rooms[(spaceStart + spaceLength - 1) - s].days[days[timeStart + t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = devices[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = devices[(spaceStart + spaceLength - 1) - s].days[days[timeStart + t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_HOUR:
                        for (int t = 0; t < timeLength; t++) yLabels[t] = hours[timeStart + t].ToString(DATETIME_FORMAT_HOUR);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = buildings[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = buildings[(spaceStart + spaceLength - 1) - s].hours[hours[timeStart + t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = rooms[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = rooms[(spaceStart + spaceLength - 1) - s].hours[hours[timeStart + t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = devices[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = devices[(spaceStart + spaceLength - 1) - s].hours[hours[timeStart + t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                return new DataSet(data, xLabels, yLabels, minValue, maxValue, currentTimeSpaceOrientation, currentTimeSpaceDirection);

            case Z_AXIS_180:
                data = new float[timeLength, spaceLength];
                xLabels = new string[timeLength];
                yLabels = new string[spaceLength];
                timeStart = (timeStart + timeLength > timeSize) ? timeSize - timeLength : timeStart;
                spaceStart = (spaceStart + spaceLength > spaceSize) ? spaceSize - spaceLength : spaceStart;
                // string[] timeIds = timeStart.Split('/');
                // string[] spaceIds = timeStart.Split('/');
                switch (timeScale)
                {
                    case TIME_SCALE_MONTH:
                        for (int t = 0; t < timeLength; t++) xLabels[t] = months[(timeStart + timeLength - 1) - t].ToString(DATETIME_FORMAT_MONTH);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = buildings[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                    {
                                        data[t, s] = buildings[(spaceStart + spaceLength - 1) - s].months[months[(timeStart + timeLength - 1) - t]];
                                    }
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = rooms[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = rooms[(spaceStart + spaceLength - 1) - s].months[months[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = devices[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = devices[(spaceStart + spaceLength - 1) - s].months[months[(timeStart + timeLength - 1) - t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_DAY:
                        for (int t = 0; t < timeLength; t++) xLabels[t] = days[(timeStart + timeLength - 1) - t].ToString(DATETIME_FORMAT_DAY);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = buildings[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = buildings[(spaceStart + spaceLength - 1) - s].days[days[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = rooms[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = rooms[(spaceStart + spaceLength - 1) - s].days[days[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = devices[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = devices[(spaceStart + spaceLength - 1) - s].days[days[(timeStart + timeLength - 1) - t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_HOUR:
                        for (int t = 0; t < timeLength; t++) xLabels[t] = hours[(timeStart + timeLength - 1) - t].ToString(DATETIME_FORMAT_HOUR);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = buildings[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = buildings[(spaceStart + spaceLength - 1) - s].hours[hours[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = rooms[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = rooms[(spaceStart + spaceLength - 1) - s].hours[hours[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) yLabels[s] = devices[(spaceStart + spaceLength - 1) - s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[t, s] = devices[(spaceStart + spaceLength - 1) - s].hours[hours[(timeStart + timeLength - 1) - t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                return new DataSet(data, xLabels, yLabels, minValue, maxValue, currentTimeSpaceOrientation, currentTimeSpaceDirection);

            case Z_AXIS_270:

                timeLength = _spaceLength;
                spaceLength = _timeLength;
                timeStart = (timeStart + timeLength > timeSize) ? timeSize - timeLength : timeStart;
                spaceStart = (spaceStart + spaceLength > spaceSize) ? spaceSize - spaceLength : spaceStart;
                data = new float[spaceLength, timeLength];
                xLabels = new string[spaceLength];
                yLabels = new string[timeLength];
                // string[] timeIds = timeStart.Split('/');
                // string[] spaceIds = timeStart.Split('/');
                switch (timeScale)
                {
                    case TIME_SCALE_MONTH:
                        for (int t = 0; t < timeLength; t++) yLabels[t] = months[(timeStart + timeLength - 1) - t].ToString(DATETIME_FORMAT_MONTH);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:

                                /*Debug.Log("Z_AXIS_270");
                                Debug.Log("xLabels.Length: " + xLabels.Length);
                                Debug.Log("spaceLength: " + spaceLength);
                                Debug.Log("(spaceStart + spaceLength - 1): " + (spaceStart + spaceLength - 1));*/
                                Debug.Log("buildings.Count: " + buildings.Count);
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = buildings[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                    {
                                        data[s, t] = buildings[spaceStart + s].months[months[(timeStart + timeLength - 1) - t]];
                                    }
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = rooms[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = rooms[spaceStart + s].months[months[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = devices[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = devices[spaceStart + s].months[months[(timeStart + timeLength - 1) - t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_DAY:
                        for (int t = 0; t < timeLength; t++) yLabels[t] = days[(timeStart + timeLength - 1) - t].ToString(DATETIME_FORMAT_DAY);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = buildings[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = buildings[spaceStart + s].days[days[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = rooms[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = rooms[spaceStart + s].days[days[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = devices[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = devices[spaceStart + s].days[days[(timeStart + timeLength - 1) - t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    case TIME_SCALE_HOUR:
                        for (int t = 0; t < timeLength; t++) yLabels[t] = hours[(timeStart + timeLength - 1) - t].ToString(DATETIME_FORMAT_HOUR);
                        switch (spaceScale)
                        {
                            case SPACE_SCALE_BUILDING:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = buildings[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = buildings[spaceStart + s].hours[hours[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_ROOM:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = rooms[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = rooms[spaceStart + s].hours[hours[(timeStart + timeLength - 1) - t]];
                                break;

                            case SPACE_SCALE_DEVICE:
                                for (int s = 0; s < spaceLength; s++) xLabels[s] = devices[spaceStart + s].name;
                                for (int t = 0; t < timeLength; t++)
                                    for (int s = 0; s < spaceLength; s++)
                                        data[s, t] = devices[spaceStart + s].hours[hours[(timeStart + timeLength - 1) - t]];
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                return new DataSet(data, xLabels, yLabels, minValue, maxValue, currentTimeSpaceOrientation, currentTimeSpaceDirection);

            default:
            return null;
        }
    }
    private void setCurrentScale(int timeScale, int spaceScale)
    {
        switch (timeScale)
        {
            case TIME_SCALE_MONTH:
                currentTimeSize = 12;
                break;
            case TIME_SCALE_DAY:
                currentTimeSize = DateTime.IsLeapYear(startDate.Year)? 366: 365;
                break;
            case TIME_SCALE_HOUR:
                currentTimeSize = (DateTime.IsLeapYear(startDate.Year) ? 366 : 365) * 24;
                break;
            default:
                break;
        }
        switch (spaceScale)
        {
            case SPACE_SCALE_BUILDING:
                currentSpaceSize = buildings.Count;
                break;
            case SPACE_SCALE_ROOM:
                currentSpaceSize = rooms.Count;
                break;
            case SPACE_SCALE_DEVICE:
                currentSpaceSize = devices.Count;
                break;
            default:
                break;
        }
    }

    /*void OnDrawGizmosSelected()
    {
        // set axis scale
        //setScale(currentTimeScale, currentSpaceScale);
        if (currentTimePos + currentTimeLength >= currentTimeSize || currentTimePos + currentTimeLength < 0) Debug.LogError("Time frame asked is out of range.");
        else if (currentSpacePos + currentSpaceLength >= currentSpaceSize || currentSpacePos + currentSpaceLength < 0) Debug.LogError("Space frame asked is out of range.");
        else
        {
            DataSet ans = FetchDataSet(currentTimeScale, currentTimePos, currentTimeLength, currentSpaceScale, currentSpacePos, currentSpaceLength);
            for (int i = 0; i < ans.data.GetLength(0); i++)
            {
                string xLabel = ans.xLabels[i];
                for (int j = 0; j < ans.data.GetLength(1); j++)
                {
                    string yLabel = ans.yLabels[j];
                    // Draw a semitransparent blue cube at the transforms position
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                    float heigthCurrentInverseLerp = Mathf.InverseLerp(ans.minValue, ans.maxValue, ans.data[i, j]);
                    float heigthCurrentLerp = Mathf.Lerp(0f, 20f, heigthCurrentInverseLerp);

                    Gizmos.DrawCube(new Vector3(i * 10, heigthCurrentLerp, j * 10), new Vector3(5, 20, 5));
                }
            }
        }
    }*/

    public DataSet DataSet()
	{
       // Debug.Log("DATASET -> [currentTimeScale("+ currentTimeScale + ")] currentTimePos (" + currentTimePos + ") + currentTimeLength(" + currentTimeLength + ") <= currentTimeSize(" + currentTimeSize + ")");
       // Debug.Log("DATASET -> [currentSpaceScale(" + currentSpaceScale + ")] currentSpacePos (" + currentSpacePos + ") + currentSpaceLength(" + currentSpaceLength + ") <= currentSpaceSize(" + currentSpaceSize + ")");
        return FetchDataSet(currentTimeScale, currentTimePos, currentTimeLength, currentTimeSize, currentSpaceScale, currentSpacePos, currentSpaceLength, currentSpaceSize);
    }


    public void Pan(int xAxisDirection, int xAxisStep,  int yAxisDirection, int yAxisStep)
	{

       /* Debug.Log("IN currentSpacePos: " + currentSpacePos);
        Debug.Log("IN currentTimePos: " + currentTimePos);
        Debug.Log("IN xAxisStep: " + xAxisStep);
        Debug.Log("IN yAxisStep: " + yAxisStep);*/
        if (xAxisDirection == X_AXIS_RIGHT && yAxisDirection == Y_AXIS_DOWN)
        {
            //Debug.Log(">PAN LEFT BOTTOM");
            // PAN LEFT BOTTOM
            currentTimePos = (currentTimePos - xAxisStep >= 0) ? currentTimePos - xAxisStep : 0;
            currentSpacePos = (currentSpacePos - yAxisStep >= 0) ? currentSpacePos - yAxisStep : 0;
        }
        if (xAxisDirection == X_AXIS_LEFT && yAxisDirection == Y_AXIS_UP)
        {
            //Debug.Log(">PAN RIGHT TOP");
            // PAN RIGHT TOP
            currentTimePos = (currentTimePos + xAxisStep + currentTimeLength < currentTimeSize) ? currentTimePos + xAxisStep : currentTimeSize - currentTimeLength - 1;
            currentSpacePos = (currentSpacePos + yAxisStep + currentSpaceLength < currentSpaceSize) ? currentSpacePos + yAxisStep : currentSpaceSize - currentSpaceLength - 1;
        }
        if (xAxisDirection == X_AXIS_LEFT && yAxisDirection == Y_AXIS_IDLE)
        {
            //Debug.Log(">PAN RIGHT");
            // PAN RIGHT
            currentTimePos = (currentTimePos + xAxisStep + currentTimeLength < currentTimeSize) ? currentTimePos + xAxisStep : currentTimeSize - currentTimeLength - 1;
        }
        if (xAxisDirection == X_AXIS_RIGHT && yAxisDirection == Y_AXIS_IDLE)
        {
            // PAN LEFT 
            //Debug.Log(">PAN LEFT");
            currentTimePos = (currentTimePos - xAxisStep >= 0) ? currentTimePos - xAxisStep : 0;
        }
        if (xAxisDirection == X_AXIS_IDLE && yAxisDirection == Y_AXIS_UP)
        {
            // PAN TOP
            /*Debug.Log(">PAN TOP");
            Debug.Log("yAxisStep: " + yAxisStep);
            Debug.Log("currentSpaceLength: " + currentSpaceLength);
            Debug.Log("currentSpaceSize: " + currentSpaceSize);*/
            currentSpacePos = (currentSpacePos + yAxisStep + currentSpaceLength < currentSpaceSize) ? currentSpacePos + yAxisStep : currentSpaceSize - currentSpaceLength - 1;
        }
        if (xAxisDirection == X_AXIS_IDLE && yAxisDirection == Y_AXIS_DOWN)
        {
            // PAN BOTTOM 
            //Debug.Log(">PAN BOTTOM");
            currentSpacePos = (currentSpacePos - yAxisStep >= 0) ? currentSpacePos - yAxisStep : 0;
        }
        /*Debug.Log("OUT currentSpacePos: " + currentSpacePos);
        Debug.Log("OUT currentTimePos: " + currentTimePos);*/
    }

    public void Zoom(int xAxisDirection, float ftimeOffset, int yAxisDirection, float fspaceOffset)
    {
        bool halfTime = (ftimeOffset % 1f != 0f);
        bool halfSpace = (fspaceOffset % 1f != 0f);
        int timeOffset = (int)ftimeOffset;
        int spaceOffset = (int)fspaceOffset;

        int localTimePos = currentTimePos + timeOffset;
        int localSpacePos = currentSpacePos + spaceOffset;

        int timeLength = currentTimeLength;
        int spaceLength = currentSpaceLength;
		/*switch (currentTimeSpaceOrientation)
		{
            case Z_AXIS_0:
                timeLength = currentTimeLength;
                spaceLength = currentSpaceLength;
            break;
            case Z_AXIS_90:
                timeLength = currentSpaceLength;
                spaceLength = currentTimeLength;
            break;
            case Z_AXIS_180:
                timeLength = currentTimeLength;
                spaceLength = currentSpaceLength;
            break;
            case Z_AXIS_270:
                timeLength = currentSpaceLength;
                spaceLength = currentTimeLength;
            break;
            default:
            break;
		}*/
        if (xAxisDirection == X_AXIS_IN && yAxisDirection == Y_AXIS_IN)
        {

			// compute new zoomed-in pos
			switch (currentTimeScale)
            {
                case TIME_SCALE_MONTH: // -> DAYS
                    //Debug.Log("zoom in -> prev currentTimePos : " + currentTimePos);
                    DateTime nDate = startDate.AddMonths(localTimePos);
                    localTimePos = nDate.DayOfYear - 1;
                    if(halfTime) localTimePos += DateTime.DaysInMonth(nDate.Year, nDate.Month)/2;
                    //Debug.Log("zoom in -> next currentTimePos : " + currentTimePos);
                    break;
                case TIME_SCALE_DAY: // -> HOURS
                    //Debug.Log("zoom in -> prev currentTimePos : " + currentTimePos);
                    localTimePos *= 24;
                    if (halfTime) localTimePos += 12;
                    //Debug.Log("zoom in -> next currentTimePos : " + currentTimePos);
                    break;
                default:
                    localTimePos = localTimePos + timeOffset;
                return;
            }
            switch (currentSpaceScale)
            {
                case SPACE_SCALE_BUILDING: // -> ROOMS
                    int nextCurrentPos = 0;
                    for (int i = 0; i < localSpacePos; i++) nextCurrentPos += buildings[i].rooms.Count;
                    if (halfSpace) nextCurrentPos += buildings[localSpacePos].rooms.Count/2;
                    //Debug.Log("zoom in -> prev currentSpacePos : " + currentSpacePos);
                    localSpacePos = nextCurrentPos;
                    //Debug.Log("zoom in -> next currentSpacePos : " + currentSpacePos);
                    break;
                case SPACE_SCALE_ROOM: // -> DEVICES
                    nextCurrentPos = 0;
                    for (int i = 0; i < localSpacePos; i++) nextCurrentPos += rooms[i].devices.Count;
                    if (halfSpace) nextCurrentPos += rooms[localSpacePos].devices.Count / 2;
                    //Debug.Log("zoom in -> prev currentTimePos : " + currentTimePos);
                    localSpacePos = nextCurrentPos;
                    //Debug.Log("zoom in -> next currentTimePos : " + currentTimePos);
                    break;
                default:
                    localSpacePos = localSpacePos + spaceOffset;
                return;
            }


            // ZOOM BOTH AXIS IN
            currentTimeScale = Math.Min(currentTimeScale + 1, TIME_SCALE_HOUR);
            currentSpaceScale = Math.Min(currentSpaceScale + 1, SPACE_SCALE_DEVICE);
            setCurrentScale(currentTimeScale, currentSpaceScale);
           /* Debug.Log("ZOOM IN > localTimePos : " + localTimePos);
            Debug.Log("ZOOM IN > timeOffset : " + timeOffset);
            Debug.Log("ZOOM IN > currentTimeLength : " + currentTimeLength);
            Debug.Log("ZOOM IN > currentTimeSize : " + currentTimeSize);
            Debug.Log("ZOOM IN > localSpacePos : " + localSpacePos);
            Debug.Log("ZOOM IN > spaceOffset : " + spaceOffset);
            Debug.Log("ZOOM IN > currentSpaceLength : " + currentSpaceLength);
            Debug.Log("ZOOM IN > currentSpaceSize : " + currentSpaceSize);*/
            currentTimePos = (localTimePos - timeOffset < 0) ? 0 : ((localTimePos - timeOffset + timeLength > currentTimeSize) ? currentTimeSize - timeLength : localTimePos - timeOffset);
            currentSpacePos = (localSpacePos - spaceOffset < 0) ? 0 : ((localSpacePos - spaceOffset + spaceLength > currentSpaceSize) ? currentSpaceSize - spaceLength : localSpacePos - spaceOffset);
            
        }
        if (xAxisDirection == X_AXIS_OUT && yAxisDirection == Y_AXIS_OUT)
        {
            // compute new zoomed-out pos
            switch (currentTimeScale)
            {
                case TIME_SCALE_DAY: // -> MONTHS
                    localTimePos = startDate.AddDays(localTimePos).Month - 1;
                    break;
                case TIME_SCALE_HOUR: // -> DAYS
                    localTimePos = (int)(localTimePos / 24f);
                    break;
                default:
                    localTimePos = localTimePos + timeOffset;
                return;
            }
            switch (currentSpaceScale)
            {
                case SPACE_SCALE_ROOM: // -> BUILDINGS
                    int nextCurrentPos = 0;
                    while ((localSpacePos -= buildings[nextCurrentPos].rooms.Count) > 0) nextCurrentPos++;
                    localSpacePos = nextCurrentPos;
                    break;
                case SPACE_SCALE_DEVICE: // -> ROOMS
                    nextCurrentPos = 0;
                    while ((localSpacePos -= rooms[nextCurrentPos].devices.Count) > 0) nextCurrentPos++;
                    localSpacePos = nextCurrentPos;
                    break;
                default:
                    localSpacePos = localSpacePos + spaceOffset;
                    return;
            }

            // ZOOM BOTH AXIS OUT
            currentTimeScale = Math.Max(currentTimeScale - 1, TIME_SCALE_MONTH);
            currentSpaceScale = Math.Max(currentSpaceScale - 1, SPACE_SCALE_BUILDING);
            setCurrentScale(currentTimeScale, currentSpaceScale);
            /*Debug.Log("ZOOM OUT > localTimePos : " + localTimePos);
            Debug.Log("ZOOM OUT > timeOffset : " + timeOffset);
            Debug.Log("ZOOM OUT > currentTimeLength : " + currentTimeLength);
            Debug.Log("ZOOM OUT > currentTimeSize : " + currentTimeSize);
            Debug.Log("ZOOM OUT > localSpacePos : " + localSpacePos);
            Debug.Log("ZOOM OUT > spaceOffset : " + spaceOffset);
            Debug.Log("ZOOM OUT > currentSpaceLength : " + currentSpaceLength);
            Debug.Log("ZOOM OUT > currentSpaceSize : " + currentSpaceSize);*/
            currentTimePos = (localTimePos - timeOffset < 0) ? 0 : ((localTimePos - timeOffset + timeLength > currentTimeSize) ? currentTimeSize - timeLength : localTimePos - timeOffset);
            currentSpacePos = (localSpacePos - spaceOffset < 0) ? 0 : ((localSpacePos - spaceOffset + spaceLength > currentSpaceSize) ? currentSpaceSize - spaceLength : localSpacePos - spaceOffset);

        }
        if (xAxisDirection == X_AXIS_IN && yAxisDirection == Y_AXIS_IDLE)
        {
            // compute new zoomed-in pos
            switch (currentTimeScale)
            {
                case TIME_SCALE_MONTH: // -> DAYS
                    //Debug.Log("zoom in -> prev currentTimePos : " + currentTimePos);
                    DateTime nDate = startDate.AddMonths(localTimePos);
                    localTimePos = nDate.DayOfYear - 1;
                    if (halfTime) localTimePos += DateTime.DaysInMonth(nDate.Year, nDate.Month) / 2;
                    //Debug.Log("zoom in -> next currentTimePos : " + currentTimePos);
                    break;
                case TIME_SCALE_DAY: // -> HOURS
                    //Debug.Log("zoom in -> prev currentTimePos : " + currentTimePos);
                    localTimePos *= 24;
                    if (halfTime) localTimePos += 12;
                    //Debug.Log("zoom in -> next currentTimePos : " + currentTimePos);
                    break;
                default:
                    localTimePos = localTimePos + timeOffset;
                return;
            }

            // ZOOM X AXIS IN
            currentTimeScale = Math.Min(currentTimeScale + 1, TIME_SCALE_HOUR);
            setCurrentScale(currentTimeScale, currentSpaceScale);

            currentTimePos = (localTimePos - timeOffset < 0) ? 0 : ((localTimePos - timeOffset + timeLength > currentTimeSize) ? currentTimeSize - timeLength : localTimePos - timeOffset);
            //currentSpaceScale = currentSpaceScale;

        }
        if (xAxisDirection == X_AXIS_OUT && yAxisDirection == Y_AXIS_IDLE)
        {
            // compute new zoomed-out pos
            switch (currentTimeScale)
            {
                case TIME_SCALE_DAY: // -> MONTHS
                    localTimePos = startDate.AddDays(localTimePos).Month - 1;
                    break;
                case TIME_SCALE_HOUR: // -> DAYS
                    localTimePos = (int)(localTimePos / 24f);
                    break;
                default:
                    localTimePos = localTimePos + timeOffset;
                return;
            }
            // ZOOM X AXIS OUT
            currentTimeScale = Math.Max(currentTimeScale - 1, TIME_SCALE_MONTH);
            setCurrentScale(currentTimeScale, currentSpaceScale);
            currentTimePos = (localTimePos - timeOffset < 0) ? 0 : ((localTimePos - timeOffset + timeLength > currentTimeSize) ? currentTimeSize - timeLength : localTimePos - timeOffset);
            //currentSpaceScale = currentSpaceScale;

        }
        if (xAxisDirection == X_AXIS_IDLE && yAxisDirection == Y_AXIS_IN)
        {
            switch (currentSpaceScale)
            {
                case SPACE_SCALE_BUILDING: // -> ROOMS
                    int nextCurrentPos = 0;
                    for (int i = 0; i < localSpacePos; i++) nextCurrentPos += buildings[i].rooms.Count;
                    if (halfSpace) nextCurrentPos += buildings[localSpacePos].rooms.Count / 2;
                    //Debug.Log("zoom in -> prev currentSpacePos : " + currentSpacePos);
                    localSpacePos = nextCurrentPos;
                    //Debug.Log("zoom in -> next currentSpacePos : " + currentSpacePos);
                    break;
                case SPACE_SCALE_ROOM: // -> DEVICES
                    nextCurrentPos = 0;
                    for (int i = 0; i < localSpacePos; i++) nextCurrentPos += rooms[i].devices.Count;
                    if (halfSpace) nextCurrentPos += rooms[localSpacePos].devices.Count / 2;
                    //Debug.Log("zoom in -> prev currentTimePos : " + currentTimePos);
                    localSpacePos = nextCurrentPos;
                    //Debug.Log("zoom in -> next currentTimePos : " + currentTimePos);
                    break;
                default:
                    localSpacePos = localSpacePos + spaceOffset;
                return;
            }
            // ZOOM Y AXIS IN
            // currentTimeScale = currentTimeScale
            currentSpaceScale = Math.Min(currentSpaceScale + 1, SPACE_SCALE_DEVICE);
            setCurrentScale(currentTimeScale, currentSpaceScale);
            currentSpacePos = (localSpacePos - spaceOffset < 0) ? 0 : ((localSpacePos - spaceOffset + spaceLength > currentSpaceSize) ? spaceLength - currentSpaceLength : localSpacePos - spaceOffset);
        }
        if (xAxisDirection == X_AXIS_IDLE && yAxisDirection == Y_AXIS_OUT)
        {
            switch (currentSpaceScale)
            {
                case SPACE_SCALE_ROOM: // -> BUILDINGS
                    int nextCurrentPos = 0;
                    while ((localSpacePos -= buildings[nextCurrentPos].rooms.Count) > 0) nextCurrentPos++;
                    localSpacePos = nextCurrentPos;
                    break;
                case SPACE_SCALE_DEVICE: // -> ROOMS
                    nextCurrentPos = 0;
                    while ( (localSpacePos -= rooms[nextCurrentPos].devices.Count) > 0) nextCurrentPos++;
                    localSpacePos = nextCurrentPos;
                    break;
                default:
                    localSpacePos = localSpacePos + spaceOffset;
                return;
            }

            // ZOOM Y AXIS OUT
            // currentTimeScale = currentTimeScale
            currentSpaceScale = Math.Max(currentSpaceScale - 1, SPACE_SCALE_BUILDING);
            setCurrentScale(currentTimeScale, currentSpaceScale);
            currentSpacePos = (localSpacePos - spaceOffset < 0) ? 0 : ((localSpacePos - spaceOffset + spaceLength > currentSpaceSize) ? currentSpaceSize - spaceLength : localSpacePos - spaceOffset);
        }
    }

    public void Rotate(int zAxisDirection)
    {
        if(zAxisDirection == Z_AXIS_CW)
        {
            currentTimeSpaceOrientation = (4 + (currentTimeSpaceOrientation + 1)) % 4;
            currentTimeSpaceDirection = Z_AXIS_CW;
        }
        if (zAxisDirection == Z_AXIS_CCW)
        {
            currentTimeSpaceOrientation = (4 + (currentTimeSpaceOrientation - 1)) % 4;
            currentTimeSpaceDirection = Z_AXIS_CCW;
        }
    }

    private void shuffle(float[] array, int repeat)
    {
		// Knuth shuffle algorithm :: courtesy of Wikipedia :)
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        for (int n = 0; n < repeat; n++)
        for (int t = 0; t < array.Length; t++)
        {
            float tmp = array[t];
            int r = UnityEngine.Random.Range(t, array.Length);
            array[t] = array[r];
            array[r] = tmp;
        }
    }

    private float[] generateShuffledDistribution(int size)
	{

        float[] distrib = new float[size];
        for (var i = 0; i < distrib.Length; i++)
        {
            if (i % (30*24) == 0) UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            if (i % 24 == 0) UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            distrib[i] = (float)rng.normal(u: 20f, s: 20f);
        }
        shuffle(distrib, 1);
        return distrib;
    }

	// Update is called once per frame
	/*void Update()
    {
        if (Input.GetKeyDown("up"))
        {
            Debug.Log("PAN UP");
            Pan(X_AXIS_IDLE, Y_AXIS_UP);
        }

        if (Input.GetKeyDown("down"))
        {
            Debug.Log("PAN DOWN");
            Pan(X_AXIS_IDLE, Y_AXIS_DOWN);
        }

        if (Input.GetKeyDown("left"))
        {
            Debug.Log("PAN LEFT");
            Pan(X_AXIS_LEFT, Y_AXIS_IDLE);
        }

        if (Input.GetKeyDown("right"))
        {
            Debug.Log("PAN RIGHT");
            Pan(X_AXIS_RIGHT, Y_AXIS_IDLE);
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Debug.Log("ZOOM IN");
            Zoom(X_AXIS_IN, Y_AXIS_IN);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Debug.Log("ZOOM OUT");
            Zoom(X_AXIS_OUT, Y_AXIS_OUT);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("ROTATE");
            Rotate(Z_AXIS_CW);
        }
    }*/
}
