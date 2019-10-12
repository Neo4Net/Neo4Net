using System;

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
namespace Org.Neo4j.causalclustering.helper
{
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.FormattedLogProvider.toOutputStream;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class StatUtil
	public class StatUtil
	{
		 public class StatContext
		 {
			  internal const int N_BUCKETS = 10; // values >= Math.pow( 10, N_BUCKETS-1 ) all go into the last bucket

			  internal readonly string Name;
			  internal readonly Log Log;
			  internal readonly long PrintEvery;
			  internal readonly bool ClearAfterPrint;
			  internal BasicStats[] Bucket = new BasicStats[N_BUCKETS];
			  internal long TotalCount;

			  internal StatContext( string name, Log log, long printEvery, bool clearAfterPrint )
			  {
					this.Name = name;
					this.Log = log;
					this.PrintEvery = printEvery;
					this.ClearAfterPrint = clearAfterPrint;
					Clear();
			  }

			  public virtual void Clear()
			  {
				  lock ( this )
				  {
						for ( int i = 0; i < N_BUCKETS; i++ )
						{
							 Bucket[i] = new BasicStats();
						}
						TotalCount = 0;
				  }
			  }

			  public virtual void Collect( double value )
			  {
					int bucketIndex = BucketFor( value );

					lock ( this )
					{
						 TotalCount++;
						 Bucket[bucketIndex].collect( value );

						 if ( TotalCount % PrintEvery == 0 )
						 {
							  Print();
						 }
					}
			  }

			  internal virtual int BucketFor( double value )
			  {
					int bucketIndex;
					if ( value <= 0 )
					{
						 // we do not have buckets for negative values, we assume user doesn't measure such things
						 // however, if they do, it will all be collected in bucket 0
						 bucketIndex = 0;
					}
					else
					{
						 bucketIndex = ( int ) Math.Log10( value );
						 bucketIndex = Math.Min( bucketIndex, N_BUCKETS - 1 );
					}
					return bucketIndex;
			  }

			  public virtual TimingContext Time()
			  {
					return new TimingContext( this );
			  }

			  public virtual void Print()
			  {
				  lock ( this )
				  {
						foreach ( BasicStats stats in Bucket )
						{
							 if ( stats.Count > 0 )
							 {
								  Log.info( format( "%s%s", Name, stats ) );
							 }
						}
      
						if ( ClearAfterPrint )
						{
							 Clear();
						}
				  }
			  }
		 }

		 private StatUtil()
		 {
		 }

		 public static StatContext Create( string name, long printEvery, bool clearAfterPrint )
		 {
			 lock ( typeof( StatUtil ) )
			 {
				  return Create( name, toOutputStream( System.out ).getLog( name ), printEvery, clearAfterPrint );
			 }
		 }

		 public static StatContext Create( string name, Log log, long printEvery, bool clearAfterPrint )
		 {
			 lock ( typeof( StatUtil ) )
			 {
				  return new StatContext( name, log, printEvery, clearAfterPrint );
			 }
		 }

		 public class TimingContext
		 {
			  internal readonly StatContext Context;
			  internal readonly long StartTime = DateTimeHelper.CurrentUnixTimeMillis();

			  internal TimingContext( StatContext context )
			  {
					this.Context = context;
			  }

			  public virtual void End()
			  {
					Context.collect( DateTimeHelper.CurrentUnixTimeMillis() - StartTime );
			  }
		 }

		 private class BasicStats
		 {
			  internal double? Min;
			  internal double? Max;

			  internal double? Avg = 0.0;
			  internal long Count;

			  internal virtual void Collect( double val )
			  {
					Count++;
					Avg = Avg + ( val - Avg ) / Count;

					Min = Min == null ? val : Math.Min( Min, val );
					Max = Max == null ? val : Math.Max( Max, val );
			  }

			  public override string ToString()
			  {
					return format( "{min=%s, max=%s, avg=%s, count=%d}", Min, Max, Avg, Count );
			  }
		 }
	}

}