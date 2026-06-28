//using NurseriesNetwork.Core.Entities;

//namespace NurseriesNetwork.Infrastructure.Data.Seed;

//public static class NurserySeeder
//{
//    public static List<Nursery> GetNurseries()
//    {
//        return new()
//        {
//            new Nursery
//{
    
//    Name = "Nile Care NICU",
//    Description = "وحدة رعاية حديثي الولادة مجهزة لاستقبال الأطفال المبتسرين وحديثي الولادة، وتوفر أجهزة تنفس صناعي، علاج ضوئي لحالات الصفراء، ومتابعة طبية على مدار 24 ساعة.",
//    DailyPrice = 850,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 35,
//    AvgRating = 4.9,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "القاهرة",
//        District = "مدينة نصر",
//        Address = "شارع مصطفى النحاس",
//        Latitude = 30.0626,
//        Longitude = 31.3369
//    }
//},

//new Nursery
//{
    
//    Name = "Life Start NICU",
//    Description = "مركز متخصص في رعاية الأطفال ناقصي الوزن، مع برامج تغذية علاجية، حضانات حديثة، وأجهزة مراقبة العلامات الحيوية.",
//    DailyPrice = 780,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 28,
//    AvgRating = 4.8,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "القاهرة",
//        District = "المعادي",
//        Address = "شارع النصر",
//        Latitude = 29.9617,
//        Longitude = 31.2575
//    }
//},

//new Nursery
//{
    
//    Name = "Hope Neonatal Unit",
//    Description = "توفر رعاية متخصصة للأطفال الذين يعانون من مشاكل التنفس بعد الولادة باستخدام أجهزة تنفس صناعي حديثة وفريق طبي متخصص.",
//    DailyPrice = 720,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 24,
//    AvgRating = 4.7,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "الجيزة",
//        District = "الدقي",
//        Address = "شارع التحرير",
//        Latitude = 30.0385,
//        Longitude = 31.2107
//    }
//},

//new Nursery
//{
//    Name = "Baby Care Plus",
//    Description = "وحدة رعاية لحديثي الولادة تقدم متابعة للأطفال المبتسرين، علاج حالات الصفراء، ورعاية متكاملة بعد الولادة المبكرة.",
//    DailyPrice = 690,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 20,
//    AvgRating = 4.6,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "الإسكندرية",
//        District = "سموحة",
//        Address = "شارع فوزي معاذ",
//        Latitude = 31.2157,
//        Longitude = 29.9555
//    }
//},

//new Nursery
//{
//    Name = "Safe Start NICU",
//    Description = "تقدم رعاية مركزة للأطفال حديثي الولادة ذوي الحالات الحرجة، مع متابعة مستمرة بواسطة استشاريي الأطفال وحديثي الولادة.",
//    DailyPrice = 640,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 18,
//    AvgRating = 4.5,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "الإسكندرية",
//        District = "لوران",
//        Address = "شارع أبو قير",
//        Latitude = 31.2451,
//        Longitude = 29.9828
//    }
//},
//new Nursery
//{
//    Name = "Bright Life NICU",
//    Description = "وحدة متخصصة في رعاية الأطفال المبتسرين ذوي الوزن المنخفض، مع أجهزة مراقبة مستمرة ودعم تغذية للأطفال حديثي الولادة.",
//    DailyPrice = 730,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 26,
//    AvgRating = 4.8,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "المنصورة",
//        District = "حي الجامعة",
//        Address = "شارع الجمهورية",
//        Latitude = 31.0416,
//        Longitude = 31.3785
//    }
//},

//new Nursery
//{
//    Name = "First Breath NICU",
//    Description = "توفر رعاية متقدمة للأطفال الذين يحتاجون إلى أجهزة تنفس صناعي بعد الولادة، مع متابعة مستمرة بواسطة استشاريي حديثي الولادة.",
//    DailyPrice = 810,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 32,
//    AvgRating = 4.9,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "طنطا",
//        District = "مركز طنطا",
//        Address = "شارع البحر",
//        Latitude = 30.7865,
//        Longitude = 31.0004
//    }
//},

//new Nursery
//{
//    Name = "Elite Neonatal Care",
//    Description = "تقدم رعاية للأطفال المصابين بالصفراء باستخدام العلاج الضوئي، بالإضافة إلى متابعة النمو والتغذية داخل الحضانة.",
//    DailyPrice = 690,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 22,
//    AvgRating = 4.6,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "الزقازيق",
//        District = "حي الزهور",
//        Address = "شارع طلبة عويضة",
//        Latitude = 30.5877,
//        Longitude = 31.5020
//    }
//},

//new Nursery
//{
//    Name = "Future Kids NICU",
//    Description = "مجهزة لاستقبال حالات الولادة المبكرة، مع حضانات حديثة، مراقبة للعلامات الحيوية، ورعاية طبية متواصلة طوال اليوم.",
//    DailyPrice = 760,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 27,
//    AvgRating = 4.7,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "الإسماعيلية",
//        District = "حي أول",
//        Address = "شارع شبين الكوم",
//        Latitude = 30.5965,
//        Longitude = 32.2715
//    }
//},

//new Nursery
//{
//    Name = "Healthy Start Unit",
//    Description = "وحدة عناية مركزة لحديثي الولادة تقدم متابعة للأطفال ناقصي الوزن، مع برامج تغذية علاجية وإشراف طبي على مدار الساعة.",
//    DailyPrice = 650,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 20,
//    AvgRating = 4.5,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "بورسعيد",
//        District = "حي الشرق",
//        Address = "شارع الجمهورية",
//        Latitude = 31.2653,
//        Longitude = 32.3019
//    }
//},
//new Nursery
//{
//    Name = "CareNest NICU",
//    Description = "وحدة رعاية حديثي الولادة مزودة بحضانات متطورة لمتابعة الأطفال المبتسرين، مع أجهزة مراقبة مستمرة للعلامات الحيوية ودعم طبي على مدار 24 ساعة.",
//    DailyPrice = 790,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 30,
//    AvgRating = 4.8,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "أسيوط",
//        District = "حي غرب",
//        Address = "شارع الجمهورية",
//        Latitude = 27.1809,
//        Longitude = 31.1837
//    }
//},

//new Nursery
//{
//    Name = "Mercy Neonatal Center",
//    Description = "متخصصة في رعاية الأطفال حديثي الولادة الذين يحتاجون إلى العلاج الضوئي لعلاج الصفراء، مع متابعة دورية من استشاريي الأطفال.",
//    DailyPrice = 680,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 22,
//    AvgRating = 4.6,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "سوهاج",
//        District = "حي شرق",
//        Address = "شارع التحرير",
//        Latitude = 26.5569,
//        Longitude = 31.6948
//    }
//},

//new Nursery
//{
//    Name = "Advanced NICU Care",
//    Description = "توفر رعاية للأطفال الذين يعانون من مشكلات التنفس بعد الولادة باستخدام أجهزة تنفس صناعي حديثة وإشراف طبي متخصص.",
//    DailyPrice = 900,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 34,
//    AvgRating = 4.9,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "الأقصر",
//        District = "وسط المدينة",
//        Address = "شارع التلفزيون",
//        Latitude = 25.6872,
//        Longitude = 32.6396
//    }
//},

//new Nursery
//{
//    Name = "Little Angels NICU",
//    Description = "تقدم رعاية شاملة للأطفال ناقصي الوزن، مع برامج تغذية علاجية، متابعة النمو، ودعم الرضاعة الطبيعية للأمهات.",
//    DailyPrice = 710,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 24,
//    AvgRating = 4.7,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "أسوان",
//        District = "حي العقاد",
//        Address = "شارع الكورنيش",
//        Latitude = 24.0889,
//        Longitude = 32.8998
//    }
//},

//new Nursery
//{
//    Name = "New Hope NICU",
//    Description = "وحدة رعاية حديثي الولادة للحالات الحرجة، مزودة بأجهزة حديثة لمراقبة القلب والتنفس، مع فريق طبي متخصص في طب حديثي الولادة.",
//    DailyPrice = 950,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 36,
//    AvgRating = 5.0,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "قنا",
//        District = "حي الحميدات",
//        Address = "شارع 23 يوليو",
//        Latitude = 26.1551,
//        Longitude = 32.7160
//    }
//},
//new Nursery
//{
//    Name = "Life Shield NICU",
//    Description = "وحدة متخصصة في متابعة الأطفال المبتسرين بعد الولادة المبكرة، مع رعاية تنفسية، متابعة الوزن، وفريق طبي متواجد على مدار الساعة.",
//    DailyPrice = 820,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 29,
//    AvgRating = 4.8,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "بني سويف",
//        District = "وسط المدينة",
//        Address = "شارع صلاح سالم",
//        Latitude = 29.0661,
//        Longitude = 31.0994
//    }
//},

//new Nursery
//{
//    Name = "Guardian Angels NICU",
//    Description = "توفر رعاية للأطفال الذين يحتاجون إلى أجهزة تنفس صناعي، مع مراقبة مستمرة لمعدل ضربات القلب ونسبة الأكسجين.",
//    DailyPrice = 870,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 31,
//    AvgRating = 4.9,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "الفيوم",
//        District = "قسم أول",
//        Address = "شارع الحرية",
//        Latitude = 29.3084,
//        Longitude = 30.8428
//    }
//},

//new Nursery
//{
//    Name = "Sunrise Neonatal Unit",
//    Description = "مجهزة لاستقبال الأطفال حديثي الولادة المصابين بالصفراء، وتقدم العلاج الضوئي مع متابعة التحاليل الطبية بشكل دوري.",
//    DailyPrice = 700,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 23,
//    AvgRating = 4.6,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "دمياط",
//        District = "رأس البر",
//        Address = "شارع النيل",
//        Latitude = 31.5085,
//        Longitude = 31.8418
//    }
//},

//new Nursery
//{
//    Name = "Tiny Hearts NICU",
//    Description = "وحدة رعاية للأطفال ناقصي الوزن تقدم برامج تغذية علاجية، متابعة النمو، ورعاية طبية متخصصة للأطفال حديثي الولادة.",
//    DailyPrice = 670,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 21,
//    AvgRating = 4.5,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "كفر الشيخ",
//        District = "وسط المدينة",
//        Address = "شارع الخليفة المأمون",
//        Latitude = 31.1117,
//        Longitude = 30.9399
//    }
//},

//new Nursery
//{
//    Name = "Prime Neonatal Care",
//    Description = "مركز متقدم للعناية المركزة بحديثي الولادة، يقدم رعاية للحالات الحرجة، أجهزة حضانات حديثة، وأطباء متخصصين في طب الأطفال وحديثي الولادة.",
//    DailyPrice = 980,
//    AgeRangeMin = 0,
//    AgeRangeMax = 3,
//    Capacity = 40,
//    AvgRating = 5.0,
//    IsVerified = true,
//    Location = new Location
//    {
//        City = "المنوفية",
//        District = "شبين الكوم",
//        Address = "شارع سعد زغلول",
//        Latitude = 30.5549,
//        Longitude = 31.0124
//    }
//},
//        };
//    }
//}