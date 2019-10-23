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
namespace Neo4Net.Kernel.impl.proc.temporal
{

	using Description = Neo4Net.Procedure.Description;
	using AnyValue = Neo4Net.Values.AnyValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTLocalTime;

	[Description("Create a LocalTime instant.")]
	internal class LocalTimeFunction : TemporalFunction<LocalTimeValue>
	{
		 internal LocalTimeFunction( System.Func<ZoneId> defaultZone ) : base( NTLocalTime, defaultZone )
		 {
		 }

		 protected internal override LocalTimeValue Now( Clock clock, string timezone, System.Func<ZoneId> defaultZone )
		 {
			  return string.ReferenceEquals( timezone, null ) ? LocalTimeValue.now( clock, defaultZone ) : LocalTimeValue.now( clock, timezone );
		 }

		 protected internal override LocalTimeValue Parse( TextValue value, System.Func<ZoneId> defaultZone )
		 {
			  return LocalTimeValue.parse( value );
		 }

		 protected internal override LocalTimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return LocalTimeValue.build( map, defaultZone );
		 }

		 protected internal override LocalTimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return LocalTimeValue.select( from, defaultZone );
		 }

		 protected internal override LocalTimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  return LocalTimeValue.truncate( unit, input, fields, defaultZone );
		 }
	}

}