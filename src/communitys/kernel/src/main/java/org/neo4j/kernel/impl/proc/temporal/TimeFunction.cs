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
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTTime;

	[Description("Create a Time instant.")]
	internal class TimeFunction : TemporalFunction<TimeValue>
	{
		 internal TimeFunction( System.Func<ZoneId> defaultZone ) : base( NTTime, defaultZone )
		 {
		 }

		 protected internal override TimeValue Now( Clock clock, string timezone, System.Func<ZoneId> defaultZone )
		 {
			  return string.ReferenceEquals( timezone, null ) ? TimeValue.now( clock, defaultZone ) : TimeValue.now( clock, timezone );
		 }

		 protected internal override TimeValue Parse( TextValue value, System.Func<ZoneId> defaultZone )
		 {
			  return TimeValue.parse( value, defaultZone );
		 }

		 protected internal override TimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return TimeValue.build( map, defaultZone );
		 }

		 protected internal override TimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return TimeValue.select( from, defaultZone );
		 }

		 protected internal override TimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  return TimeValue.truncate( unit, input, fields, defaultZone );
		 }
	}

}