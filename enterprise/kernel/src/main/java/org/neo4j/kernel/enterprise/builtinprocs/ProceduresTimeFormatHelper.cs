/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.enterprise.builtinprocs
{


	public class ProceduresTimeFormatHelper
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static String formatTime(final long startTime, java.time.ZoneId zoneId)
		 internal static string FormatTime( long startTime, ZoneId zoneId )
		 {
			  return OffsetDateTime.ofInstant( Instant.ofEpochMilli( startTime ), zoneId ).format( ISO_OFFSET_DATE_TIME );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static String formatInterval(final long l)
		 internal static string FormatInterval( long l )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long hr = MILLISECONDS.toHours(l);
			  long hr = MILLISECONDS.toHours( l );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long min = MILLISECONDS.toMinutes(l - HOURS.toMillis(hr));
			  long min = MILLISECONDS.toMinutes( l - HOURS.toMillis( hr ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long sec = MILLISECONDS.toSeconds(l - HOURS.toMillis(hr) - MINUTES.toMillis(min));
			  long sec = MILLISECONDS.toSeconds( l - HOURS.toMillis( hr ) - MINUTES.toMillis( min ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ms = l - HOURS.toMillis(hr) - MINUTES.toMillis(min) - SECONDS.toMillis(sec);
			  long ms = l - HOURS.toMillis( hr ) - MINUTES.toMillis( min ) - SECONDS.toMillis( sec );
			  return string.Format( "{0:D2}:{1:D2}:{2:D2}.{3:D3}", hr, min, sec, ms );
		 }

	}

}