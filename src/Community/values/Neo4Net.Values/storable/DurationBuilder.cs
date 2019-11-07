using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Values.Storable
{
	using Neo4Net.Values;

	internal abstract class DurationBuilder<Input, Result> : StructureBuilder<Input, Result>
	{
		public abstract T Build( StructureBuilder<AnyValue, T> builder, IEnumerable<KeyValuePair<string, AnyValue>> entries );
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public abstract T build(final Neo4Net.values.StructureBuilder<AnyValue, T> builder, Neo4Net.values.virtual.MapValue map);
		public abstract T Build( StructureBuilder<AnyValue, T> builder, Neo4Net.Values.@virtual.MapValue map );
		 private Input _years;
		 private Input _months;
		 private Input _weeks;
		 private Input _days;
		 private Input _hours;
		 private Input _minutes;
		 private Input _seconds;
		 private Input _milliseconds;
		 private Input _microseconds;
		 private Input _nanoseconds;

		 public override StructureBuilder<Input, Result> Add( string field, Input value )
		 {
			  switch ( field.ToLower() )
			  {
			  case "years":
					this._years = value;
					break;
			  case "months":
					this._months = value;
					break;
			  case "weeks":
					this._weeks = value;
					break;
			  case "days":
					this._days = value;
					break;
			  case "hours":
					this._hours = value;
					break;
			  case "minutes":
					this._minutes = value;
					break;
			  case "seconds":
					this._seconds = value;
					break;
			  case "milliseconds":
					this._milliseconds = value;
					break;
			  case "microseconds":
					this._microseconds = value;
					break;
			  case "nanoseconds":
					this._nanoseconds = value;
					break;
			  default:
					throw new System.InvalidOperationException( "Unknown field: " + field );
			  }
			  return this;
		 }

		 public override Result Build()
		 {
			  return Create( _years, _months, _weeks, _days, _hours, _minutes, _seconds, _milliseconds, _microseconds, _nanoseconds );
		 }

		 internal abstract Result Create( Input years, Input months, Input weeks, Input days, Input hours, Input minutes, Input seconds, Input milliseconds, Input microseconds, Input nanoseconds );
	}

}