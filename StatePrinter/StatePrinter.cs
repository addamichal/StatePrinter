﻿// Copyright 2014 Kasper B. Graversen
// 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
using System;

using StatePrinter.Configurations;
using StatePrinter.FieldHarvesters;
using StatePrinter.Introspection;
using StatePrinter.TestAssistance;

namespace StatePrinter
{
    /// <summary>
    /// A class able to dump an object graph to a string.
    /// class inheritance hierarchies as well as cyclic graphs are supported.
    /// 
    /// Simply call the <see cref="PrintObject"/> with an object or object-graph to get a string representation.
    /// 
    /// The printing process is thread safe.
    /// 
    /// The state printer is highly customizable both in terms of which types and fields are printed as well as how printing of values is done.
    /// </summary>
    public class Stateprinter
    {
        readonly Configuration configuration;
        Asserter asserter;

        /// <summary>
        /// The cache cannot be static since we have many different harvesters, and potentially many different usages of <see cref="ProjectionHarvester"/>
        /// </summary>
        readonly HarvestInfoCache harvestCache = new HarvestInfoCache();

        /// <summary>
        /// Create an state printer using the supplied configuration.
        /// </summary>
        public Stateprinter(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.configuration = configuration;
        }

        /// <summary>
        /// Create an state printer using the <see cref="ConfigurationHelper.GetStandardConfiguration"/>
        /// </summary>
        public Stateprinter() : this(ConfigurationHelper.GetStandardConfiguration())
        {
        }

        /// <summary>
        /// Print an object graph to a string.
        /// </summary>
        /// <param name="objectToPrint">What to print.</param>
        /// <param name="rootname">The name of the root as it is printed.</param>
        public string PrintObject(object objectToPrint, string rootname = null)
        {
            var introSpector = new IntroSpector(configuration, harvestCache);
            var tokens = introSpector.PrintObject(objectToPrint, rootname);

            var formatter = configuration.OutputFormatter;

            var result = formatter.Print(tokens);

            if (configuration.OutputAsSingleLine) 
                result = result
                    .Replace("\r\n", " ")
                    .Replace("\r", " ")
                    .Replace("\n", " ")
                    .TrimEnd();

            return result;
        }

        public Asserter Assert
        {
            get
            {
                // lazy fetch so not to require people to set up an asserter when it is not used
                if (asserter == null)
                {
                    if (configuration.AreEqualsMethod == null)
                    {
                        var message =
                            "The configuration has no value for AreEqualsMethod which is to point to your testing framework, "
                            + "e.g. use the value: 'Assert.AreEqual' "
                            + "or the more long-winded: '(expected, actual, msg) => Assert.AreEqual(expected, actual, msg)'.\r\n"
                            + "Parameter name: Configuration.AreEqualsMethod";
                        throw new ArgumentNullException("Configuration.AreEqualsMethod", message);
                    }
                    asserter = new Asserter(configuration.AreEqualsMethod);
                }

                return asserter;
            }
        }

    }
}
