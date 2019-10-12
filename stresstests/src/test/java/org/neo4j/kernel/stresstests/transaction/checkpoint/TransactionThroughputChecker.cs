using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.stresstests.transaction.checkpoint
{

	using Workload = Org.Neo4j.Kernel.stresstests.transaction.checkpoint.workload.Workload;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TransactionThroughputChecker : Workload.TransactionThroughput
	{
		 private readonly DateFormat _dateFormat = NewDateFormat();
		 private readonly IDictionary<string, double> _reports = new LinkedHashMap<string, double>();

		 public override void Report( long transactions, long timeSlotMillis )
		 {
			  long elapsedSeconds = TimeUnit.MILLISECONDS.toSeconds( timeSlotMillis );
			  double throughput = ( double ) transactions / ( double ) elapsedSeconds;
			  string timestamp = CurrentTime();
			  _reports[timestamp] = throughput;
		 }

		 public virtual void AssertThroughput( PrintStream @out )
		 {
			  if ( _reports.Count == 0 )
			  {
					@out.println( "no reports" );
					return;
			  }

			  PrintThroughputReports( @out );

			  double average = average( _reports.Values );
			  @out.println( "Average throughput (tx/s): " + average );

			  double stdDeviation = stdDeviation( _reports.Values );
			  @out.println( "Standard deviation (tx/s): " + stdDeviation );
			  double twoStdDeviations = stdDeviation * 2.0;
			  @out.println( "Two standard deviations (tx/s): " + twoStdDeviations );

			  int inOneStdDeviationRange = 0;
			  int inTwoStdDeviationRange = 0;
			  foreach ( double report in _reports.Values )
			  {
					if ( Math.Abs( average - report ) <= stdDeviation )
					{
						 inOneStdDeviationRange++;
						 inTwoStdDeviationRange++;
					}
					else if ( Math.Abs( average - report ) <= twoStdDeviations )
					{
						 @out.println( "Outside _one_ std deviation range: " + report );
						 inTwoStdDeviationRange++;
					}
					else
					{
						 @out.println( "Outside _two_ std deviation range: " + report );
					}
			  }

			  int inOneStdDeviationRangePercentage = ( int )( ( inOneStdDeviationRange * 100.0 ) / ( double ) _reports.Count );
			  Console.WriteLine( "Percentage inside one std deviation is: " + inOneStdDeviationRangePercentage );
			  assertTrue( "Assumption is that at least 60 percent should be in one std deviation (" + stdDeviation + ")" + " range from the average (" + average + ") ", inOneStdDeviationRangePercentage >= 60 );

			  int inTwoStdDeviationRangePercentage = ( int )( ( inTwoStdDeviationRange * 100.0 ) / ( double ) _reports.Count );
			  Console.WriteLine( "Percentage inside two std deviations is: " + inTwoStdDeviationRangePercentage );
			  assertTrue( "Assumption is that at least 90 percent should be in two std deviations (" + twoStdDeviations + ")" + " range from the average (" + average + ") ", inTwoStdDeviationRangePercentage >= 90 );
		 }

		 private void PrintThroughputReports( PrintStream @out )
		 {
			  @out.println( "Throughput reports (tx/s):" );

			  foreach ( KeyValuePair<string, double> entry in _reports.SetOfKeyValuePairs() )
			  {
					@out.println( "\t" + entry.Key + "  " + entry.Value );
			  }

			  @out.println();
		 }

		 private static double Average( ICollection<double> values )
		 {
			  double sum = 0;
			  foreach ( double? value in values )
			  {
					sum += value.Value;
			  }
			  return sum / values.Count;
		 }

		 private static double StdDeviation( ICollection<double> values )
		 {
			  double average = average( values );
			  double powerSum = 0;
			  foreach ( double value in values )
			  {
					powerSum += Math.Pow( value - average, 2 );
			  }
			  return Math.Sqrt( powerSum / ( double ) values.Count );
		 }

		 private string CurrentTime()
		 {
			  return _dateFormat.format( DateTime.Now );
		 }

		 private static DateFormat NewDateFormat()
		 {
			  DateFormat format = new SimpleDateFormat( "yyyy-MM-dd HH:mm:ss.SSSZ" );
			  TimeZone timeZone = TimeZone.getTimeZone( "UTC" );
			  format.TimeZone = timeZone;
			  return format;
		 }
	}

}