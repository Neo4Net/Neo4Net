using System.Collections.Generic;
using System.IO;

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
namespace Org.Neo4j.metrics
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class MetricsTestHelper
	{
		 internal interface CsvField
		 {
			  string Header();
		 }

		 internal sealed class GaugeField : CsvField
		 {
			  public static readonly GaugeField TimeStamp = new GaugeField( "TimeStamp", InnerEnum.TimeStamp, "t" );
			  public static readonly GaugeField MetricsValue = new GaugeField( "MetricsValue", InnerEnum.MetricsValue, "value" );

			  private static readonly IList<GaugeField> valueList = new List<GaugeField>();

			  static GaugeField()
			  {
				  valueList.Add( TimeStamp );
				  valueList.Add( MetricsValue );
			  }

			  public enum InnerEnum
			  {
				  TimeStamp,
				  MetricsValue
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal GaugeField( string name, InnerEnum innerEnum, string header )
			  {
					this._header = header;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public string Header()
			  {
					return _header;
			  }

			 public static IList<GaugeField> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static GaugeField valueOf( string name )
			 {
				 foreach ( GaugeField enumInstance in GaugeField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal sealed class TimerField : CsvField
		 {
			  public static readonly TimerField T = new TimerField( "T", InnerEnum.T );
			  public static readonly TimerField Count = new TimerField( "Count", InnerEnum.Count );
			  public static readonly TimerField Max = new TimerField( "Max", InnerEnum.Max );
			  public static readonly TimerField Mean = new TimerField( "Mean", InnerEnum.Mean );
			  public static readonly TimerField Min = new TimerField( "Min", InnerEnum.Min );
			  public static readonly TimerField Stddev = new TimerField( "Stddev", InnerEnum.Stddev );
			  public static readonly TimerField P50 = new TimerField( "P50", InnerEnum.P50 );
			  public static readonly TimerField P75 = new TimerField( "P75", InnerEnum.P75 );
			  public static readonly TimerField P95 = new TimerField( "P95", InnerEnum.P95 );
			  public static readonly TimerField P98 = new TimerField( "P98", InnerEnum.P98 );
			  public static readonly TimerField P99 = new TimerField( "P99", InnerEnum.P99 );
			  public static readonly TimerField P999 = new TimerField( "P999", InnerEnum.P999 );
			  public static readonly TimerField MeanRate = new TimerField( "MeanRate", InnerEnum.MeanRate );
			  public static readonly TimerField M1Rate = new TimerField( "M1Rate", InnerEnum.M1Rate );
			  public static readonly TimerField M5Rate = new TimerField( "M5Rate", InnerEnum.M5Rate );
			  public static readonly TimerField M15Rate = new TimerField( "M15Rate", InnerEnum.M15Rate );
			  public static readonly TimerField RateUnit = new TimerField( "RateUnit", InnerEnum.RateUnit );
			  public static readonly TimerField DurationUnit = new TimerField( "DurationUnit", InnerEnum.DurationUnit );

			  private static readonly IList<TimerField> valueList = new List<TimerField>();

			  static TimerField()
			  {
				  valueList.Add( T );
				  valueList.Add( Count );
				  valueList.Add( Max );
				  valueList.Add( Mean );
				  valueList.Add( Min );
				  valueList.Add( Stddev );
				  valueList.Add( P50 );
				  valueList.Add( P75 );
				  valueList.Add( P95 );
				  valueList.Add( P98 );
				  valueList.Add( P99 );
				  valueList.Add( P999 );
				  valueList.Add( MeanRate );
				  valueList.Add( M1Rate );
				  valueList.Add( M5Rate );
				  valueList.Add( M15Rate );
				  valueList.Add( RateUnit );
				  valueList.Add( DurationUnit );
			  }

			  public enum InnerEnum
			  {
				  T,
				  Count,
				  Max,
				  Mean,
				  Min,
				  Stddev,
				  P50,
				  P75,
				  P95,
				  P98,
				  P99,
				  P999,
				  MeanRate,
				  M1Rate,
				  M5Rate,
				  M15Rate,
				  RateUnit,
				  DurationUnit
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private TimerField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public string Header()
			  {
					return name().ToLower();
			  }

			 public static IList<TimerField> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static TimerField valueOf( string name )
			 {
				 foreach ( TimerField enumInstance in TimerField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private MetricsTestHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long readLongValue(java.io.File metricFile) throws java.io.IOException, InterruptedException
		 public static long ReadLongValue( File metricFile )
		 {
			  return ReadLongValueAndAssert( metricFile, ( one, two ) => true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long readLongValueAndAssert(java.io.File metricFile, System.Func<long,long, boolean> assumption) throws java.io.IOException, InterruptedException
		 public static long ReadLongValueAndAssert( File metricFile, System.Func<long, long, bool> assumption )
		 {
			  return ReadValueAndAssert( metricFile, 0L, GaugeField.TimeStamp, GaugeField.MetricsValue, long?.parseLong, assumption );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static double readDoubleValue(java.io.File metricFile) throws java.io.IOException, InterruptedException
		 internal static double ReadDoubleValue( File metricFile )
		 {
			  return ReadValueAndAssert( metricFile, 0d, GaugeField.TimeStamp, GaugeField.MetricsValue, double?.parseDouble, ( one, two ) => true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static long readTimerLongValue(java.io.File metricFile, TimerField field) throws java.io.IOException, InterruptedException
		 internal static long ReadTimerLongValue( File metricFile, TimerField field )
		 {
			  return ReadTimerLongValueAndAssert( metricFile, ( a, b ) => true, field );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static long readTimerLongValueAndAssert(java.io.File metricFile, System.Func<long,long, boolean> assumption, TimerField field) throws java.io.IOException, InterruptedException
		 internal static long ReadTimerLongValueAndAssert( File metricFile, System.Func<long, long, bool> assumption, TimerField field )
		 {
			  return ReadValueAndAssert( metricFile, 0L, TimerField.T, field, long?.parseLong, assumption );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static double readTimerDoubleValue(java.io.File metricFile, TimerField field) throws java.io.IOException, InterruptedException
		 internal static double ReadTimerDoubleValue( File metricFile, TimerField field )
		 {
			  return ReadTimerDoubleValueAndAssert( metricFile, ( a, b ) => true, field );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static double readTimerDoubleValueAndAssert(java.io.File metricFile, System.Func<double,double, boolean> assumption, TimerField field) throws java.io.IOException, InterruptedException
		 internal static double ReadTimerDoubleValueAndAssert( File metricFile, System.Func<double, double, bool> assumption, TimerField field )
		 {
			  return ReadValueAndAssert( metricFile, 0d, TimerField.T, field, double?.parseDouble, assumption );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <T, FIELD extends Enum<FIELD> & CsvField> T readValueAndAssert(java.io.File metricFile, T startValue, FIELD timeStampField, FIELD metricsValue, System.Func<String,T> parser, System.Func<T,T, boolean> assumption) throws java.io.IOException, InterruptedException
		 private static T ReadValueAndAssert<T, FIELD>( File metricFile, T startValue, FIELD timeStampField, FIELD metricsValue, System.Func<string, T> parser, System.Func<T, T, bool> assumption ) where FIELD : Enum<FIELD>, CsvField
		 {
			  // let's wait until the file is in place (since the reporting is async that might take a while)
			  assertEventually( "Metrics file should exist", metricFile.exists, @is( true ), 40, SECONDS );

			  using ( StreamReader reader = new StreamReader( metricFile ) )
			  {
					string s;
					do
					{
						 s = reader.ReadLine();
					} while ( string.ReferenceEquals( s, null ) );
					string[] headers = s.Split( ",", true );
					assertThat( headers.Length, @is( timeStampField.GetType().EnumConstants.length ) );
					assertThat( headers[timeStampField.ordinal()], @is(timeStampField.header()) );
					assertThat( headers[metricsValue.ordinal()], @is(metricsValue.header()) );

					T currentValue = startValue;
					string line;

					// Always read at least one line of data
					do
					{
						 line = reader.ReadLine();
					} while ( string.ReferenceEquals( line, null ) );

					do
					{
						 string[] fields = line.Split( ",", true );
						 T newValue = parser( fields[metricsValue.ordinal()] );
						 assertTrue( "assertion failed on " + newValue + " " + currentValue, assumption( newValue, currentValue ) );
						 currentValue = newValue;
					} while ( !string.ReferenceEquals( ( line = reader.ReadLine() ), null ) );
					return currentValue;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File metricsCsv(java.io.File dbDir, String metric) throws InterruptedException
		 public static File MetricsCsv( File dbDir, string metric )
		 {
			  File csvFile = new File( dbDir, metric + ".csv" );
			  assertEventually( "Metrics file should exist", csvFile.exists, @is( true ), 40, SECONDS );
			  return csvFile;
		 }
	}

}