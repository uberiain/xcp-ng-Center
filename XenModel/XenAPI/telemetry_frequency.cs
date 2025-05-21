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

using Newtonsoft.Json;


namespace XenAPI
{
    [JsonConverter(typeof(telemetry_frequencyConverter))]
    public enum telemetry_frequency
    {
        /// <summary>
        /// Run telemetry task daily
        /// </summary>
        daily,
        /// <summary>
        /// Run telemetry task weekly
        /// </summary>
        weekly,
        /// <summary>
        /// Run telemetry task monthly
        /// </summary>
        monthly,
        unknown
    }

    public static class telemetry_frequency_helper
    {
        public static string ToString(telemetry_frequency x)
        {
            return x.StringOf();
        }
    }

    public static partial class EnumExt
    {
        public static string StringOf(this telemetry_frequency x)
        {
            switch (x)
            {
                case telemetry_frequency.daily:
                    return "daily";
                case telemetry_frequency.weekly:
                    return "weekly";
                case telemetry_frequency.monthly:
                    return "monthly";
                default:
                    return "unknown";
            }
        }
    }

    internal class telemetry_frequencyConverter : XenEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((telemetry_frequency)value).StringOf());
        }
    }
}