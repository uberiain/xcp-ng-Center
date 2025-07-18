/*
 * Copyright (c) Cloud Software Group, Inc.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 *   1) Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *
 *   2) Redistributions in binary form must reproduce the above
 *      copyright notice, this list of conditions and the following
 *      disclaimer in the documentation and/or other materials
 *      provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;


namespace XenAPI
{
    /// <summary>
    /// Data sources for logging in RRDs
    /// First published in XenServer 5.0.
    /// </summary>
    public partial class Data_source : XenObject<Data_source>
    {
        #region Constructors

        public Data_source()
        {
        }

        public Data_source(string name_label,
            string name_description,
            bool enabled,
            bool standard,
            string units,
            double min,
            double max,
            double value)
        {
            this.name_label = name_label;
            this.name_description = name_description;
            this.enabled = enabled;
            this.standard = standard;
            this.units = units;
            this.min = min;
            this.max = max;
            this.value = value;
        }

        /// <summary>
        /// Creates a new Data_source from a Hashtable.
        /// Note that the fields not contained in the Hashtable
        /// will be created with their default values.
        /// </summary>
        /// <param name="table"></param>
        public Data_source(Hashtable table)
            : this()
        {
            UpdateFrom(table);
        }

        #endregion

        /// <summary>
        /// Updates each field of this instance with the value of
        /// the corresponding field of a given Data_source.
        /// </summary>
        public override void UpdateFrom(Data_source record)
        {
            name_label = record.name_label;
            name_description = record.name_description;
            enabled = record.enabled;
            standard = record.standard;
            units = record.units;
            min = record.min;
            max = record.max;
            value = record.value;
        }

        /// <summary>
        /// Given a Hashtable with field-value pairs, it updates the fields of this Data_source
        /// with the values listed in the Hashtable. Note that only the fields contained
        /// in the Hashtable will be updated and the rest will remain the same.
        /// </summary>
        /// <param name="table"></param>
        public void UpdateFrom(Hashtable table)
        {
            if (table.ContainsKey("name_label"))
                name_label = Marshalling.ParseString(table, "name_label");
            if (table.ContainsKey("name_description"))
                name_description = Marshalling.ParseString(table, "name_description");
            if (table.ContainsKey("enabled"))
                enabled = Marshalling.ParseBool(table, "enabled");
            if (table.ContainsKey("standard"))
                standard = Marshalling.ParseBool(table, "standard");
            if (table.ContainsKey("units"))
                units = Marshalling.ParseString(table, "units");
            if (table.ContainsKey("min"))
                min = Marshalling.ParseDouble(table, "min");
            if (table.ContainsKey("max"))
                max = Marshalling.ParseDouble(table, "max");
            if (table.ContainsKey("value"))
                value = Marshalling.ParseDouble(table, "value");
        }

        public bool DeepEquals(Data_source other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Helper.AreEqual2(_name_label, other._name_label) &&
                Helper.AreEqual2(_name_description, other._name_description) &&
                Helper.AreEqual2(_enabled, other._enabled) &&
                Helper.AreEqual2(_standard, other._standard) &&
                Helper.AreEqual2(_units, other._units) &&
                Helper.AreEqual2(_min, other._min) &&
                Helper.AreEqual2(_max, other._max) &&
                Helper.AreEqual2(_value, other._value);
        }

        public override string SaveChanges(Session session, string opaqueRef, Data_source server)
        {
            if (opaqueRef == null)
            {
                System.Diagnostics.Debug.Assert(false, "Cannot create instances of this type on the server");
                return "";
            }
            else
            {
              throw new InvalidOperationException("This type has no read/write properties");
            }
        }

        /// <summary>
        /// a human-readable name
        /// </summary>
        public virtual string name_label
        {
            get { return _name_label; }
            set
            {
                if (!Helper.AreEqual(value, _name_label))
                {
                    _name_label = value;
                    NotifyPropertyChanged("name_label");
                }
            }
        }
        private string _name_label = "";

        /// <summary>
        /// a notes field containing human-readable description
        /// </summary>
        public virtual string name_description
        {
            get { return _name_description; }
            set
            {
                if (!Helper.AreEqual(value, _name_description))
                {
                    _name_description = value;
                    NotifyPropertyChanged("name_description");
                }
            }
        }
        private string _name_description = "";

        /// <summary>
        /// true if the data source is being logged
        /// </summary>
        public virtual bool enabled
        {
            get { return _enabled; }
            set
            {
                if (!Helper.AreEqual(value, _enabled))
                {
                    _enabled = value;
                    NotifyPropertyChanged("enabled");
                }
            }
        }
        private bool _enabled;

        /// <summary>
        /// true if the data source is enabled by default. Non-default data sources cannot be disabled
        /// </summary>
        public virtual bool standard
        {
            get { return _standard; }
            set
            {
                if (!Helper.AreEqual(value, _standard))
                {
                    _standard = value;
                    NotifyPropertyChanged("standard");
                }
            }
        }
        private bool _standard;

        /// <summary>
        /// the units of the value
        /// </summary>
        public virtual string units
        {
            get { return _units; }
            set
            {
                if (!Helper.AreEqual(value, _units))
                {
                    _units = value;
                    NotifyPropertyChanged("units");
                }
            }
        }
        private string _units = "";

        /// <summary>
        /// the minimum value of the data source
        /// </summary>
        public virtual double min
        {
            get { return _min; }
            set
            {
                if (!Helper.AreEqual(value, _min))
                {
                    _min = value;
                    NotifyPropertyChanged("min");
                }
            }
        }
        private double _min;

        /// <summary>
        /// the maximum value of the data source
        /// </summary>
        public virtual double max
        {
            get { return _max; }
            set
            {
                if (!Helper.AreEqual(value, _max))
                {
                    _max = value;
                    NotifyPropertyChanged("max");
                }
            }
        }
        private double _max;

        /// <summary>
        /// current value of the data source
        /// </summary>
        public virtual double value
        {
            get { return _value; }
            set
            {
                if (!Helper.AreEqual(value, _value))
                {
                    _value = value;
                    NotifyPropertyChanged("value");
                }
            }
        }
        private double _value;
    }
}
