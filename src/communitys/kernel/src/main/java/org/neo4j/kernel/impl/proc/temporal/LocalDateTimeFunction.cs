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
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTLocalDateTime;

	[Description("Create a LocalDateTime instant.")]
	internal class LocalDateTimeFunction : TemporalFunction<LocalDateTimeValue>
	{
		 internal LocalDateTimeFunction( System.Func<ZoneId> defaultZone ) : base( NTLocalDateTime, defaultZone )
		 {
		 }

		 protected internal override LocalDateTimeValue Now( Clock clock, string timezone, System.Func<ZoneId> defaultZone )
		 {
			  return string.ReferenceEquals( timezone, null ) ? LocalDateTimeValue.now( clock, defaultZone ) : LocalDateTimeValue.now( clock, timezone );
		 }

		 protected internal override LocalDateTimeValue Parse( TextValue value, System.Func<ZoneId> defaultZone )
		 {
			  return LocalDateTimeValue.parse( value );
		 }

		 protected internal override LocalDateTimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return LocalDateTimeValue.build( map, defaultZone );
		 }

		 protected internal override LocalDateTimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return LocalDateTimeValue.select( from, defaultZone );
		 }

		 protected internal override LocalDateTimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  return LocalDateTimeValue.truncate( unit, input, fields, defaultZone );
		 }
	}

}