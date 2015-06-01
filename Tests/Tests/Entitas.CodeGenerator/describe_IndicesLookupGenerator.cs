﻿using NSpec;
using Entitas.CodeGenerator;
using System;
using System.Linq;

class describe_IndicesLookupGenerator : nspec {
    const string defaultLookupName = "ComponentIds";
    bool logResults = false;

    void generates(Type type, string lookupName, string lookupCode) {
        generates(new [] { type }, lookupName, lookupCode);
    }

    void generates(Type[] types, string lookupName, string lookupCode) {
        var files = new IndicesLookupGenerator().Generate(types);
        files.Length.should_be(1);

        files.Any(f => f.fileName == lookupName).should_be_true();
        
        var file = files.First(f => f.fileName == lookupName);

        if (logResults) {
            Console.WriteLine("should:\n" + lookupCode);
            Console.WriteLine("was:\n" + file.fileContent);
        }

        file.fileContent.should_be(lookupCode);
    }

    const string defaultTagCode = @"

namespace Entitas {
    public partial class Matcher : AllOfMatcher {
        public Matcher(int index) : base(new [] { index }) {
        }

        public override string ToString() {
            return ComponentIds.IdToString(indices[0]);
        }
    }
}";

    void when_generating() {



        it["generates default lookup"] = () => {
            generates(typeof(SomeComponent), defaultLookupName,
                @"using System.Collections.Generic;

public static class ComponentIds {
    public const int Some = 0;

    public const int TotalComponents = 1;

    static readonly Dictionary<int, string> components = new Dictionary<int, string> {
        { 0, ""Some"" }
    };

    public static string IdToString(int componentId) {
        return components[componentId];
    }
}" + defaultTagCode);
        };



        it["generates lookup with name from attribute"] = () => {
            generates(typeof(OtherPoolComponent), "OtherComponentIds",
                @"using Entitas;

using System.Collections.Generic;

public static class OtherComponentIds {
    public const int OtherPool = 0;

    public const int TotalComponents = 1;

    static readonly Dictionary<int, string> components = new Dictionary<int, string> {
        { 0, ""OtherPool"" }
    };

    public static string IdToString(int componentId) {
        return components[componentId];
    }
}

public partial class OtherMatcher : AllOfMatcher {
    public OtherMatcher(int index) : base(new [] { index }) {
    }

    public override string ToString() {
        return OtherComponentIds.IdToString(indices[0]);
    }
}");
        };



        it["generates id for [DontGenerate]"] = () => {
            generates(typeof(DontGenerateComponent), defaultLookupName,
                @"using System.Collections.Generic;

public static class ComponentIds {
    public const int DontGenerate = 0;

    public const int TotalComponents = 1;

    static readonly Dictionary<int, string> components = new Dictionary<int, string> {
        { 0, ""DontGenerate"" }
    };

    public static string IdToString(int componentId) {
        return components[componentId];
    }
}" + defaultTagCode);
        };



        it["generates ids for all types ordered alphabetically"] = () => {
            generates(new [] {
                typeof(SomeComponent),
                typeof(DontGenerateComponent)
            }, defaultLookupName,
                @"using System.Collections.Generic;

public static class ComponentIds {
    public const int DontGenerate = 0;
    public const int Some = 1;

    public const int TotalComponents = 2;

    static readonly Dictionary<int, string> components = new Dictionary<int, string> {
        { 0, ""DontGenerate"" },
        { 1, ""Some"" }
    };

    public static string IdToString(int componentId) {
        return components[componentId];
    }
}" + defaultTagCode);
        };



        it["ignores [DontGenerate(false)]"] = () => {
            generates(new [] {
                typeof(SomeComponent),
                typeof(DontGenerateIndexComponent)
            }, defaultLookupName,
                @"using System.Collections.Generic;

public static class ComponentIds {
    public const int Some = 0;

    public const int TotalComponents = 1;

    static readonly Dictionary<int, string> components = new Dictionary<int, string> {
        { 0, ""Some"" }
    };

    public static string IdToString(int componentId) {
        return components[componentId];
    }
}" + defaultTagCode);
        };
    }
}

