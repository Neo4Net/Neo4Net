/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Io.pagecache.stress
{

	using PageCacheCounters = Org.Neo4j.Io.pagecache.monitoring.PageCacheCounters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	public class Conditions
	{
		 private Conditions()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Condition numberOfEvictions(final org.neo4j.io.pagecache.monitoring.PageCacheCounters monitor, final long desiredNumberOfEvictions)
		 public static Condition NumberOfEvictions( PageCacheCounters monitor, long desiredNumberOfEvictions )
		 {
			  return () => monitor.Evictions() > desiredNumberOfEvictions;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Condition timePeriod(final int duration, final java.util.concurrent.TimeUnit timeUnit)
		 public static Condition TimePeriod( int duration, TimeUnit timeUnit )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long endTimeInMilliseconds = currentTimeMillis() + timeUnit.toMillis(duration);
			  long endTimeInMilliseconds = currentTimeMillis() + timeUnit.toMillis(duration);

			  return () => currentTimeMillis() > endTimeInMilliseconds;
		 }
	}

}