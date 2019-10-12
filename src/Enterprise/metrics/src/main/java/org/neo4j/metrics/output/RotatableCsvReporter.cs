using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
namespace Neo4Net.metrics.output
{
	using Clock = com.codahale.metrics.Clock;
	using Counter = com.codahale.metrics.Counter;
	using Gauge = com.codahale.metrics.Gauge;
	using Histogram = com.codahale.metrics.Histogram;
	using Meter = com.codahale.metrics.Meter;
	using MetricFilter = com.codahale.metrics.MetricFilter;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using ScheduledReporter = com.codahale.metrics.ScheduledReporter;
	using Snapshot = com.codahale.metrics.Snapshot;
	using Timer = com.codahale.metrics.Timer;


	using IOUtils = Neo4Net.Io.IOUtils;
	using RotatingFileOutputStreamSupplier = Neo4Net.Logging.RotatingFileOutputStreamSupplier;

	public class RotatableCsvReporter : ScheduledReporter
	{
		 private readonly Locale _locale;
		 private readonly Clock _clock;
		 private readonly File _directory;
		 private readonly IDictionary<File, CsvRotatableWriter> _writers;
		 private readonly System.Func<File, RotatingFileOutputStreamSupplier.RotationListener, RotatingFileOutputStreamSupplier> _fileSupplierStreamCreator;

		 internal RotatableCsvReporter( MetricRegistry registry, Locale locale, TimeUnit rateUnit, TimeUnit durationUnit, Clock clock, File directory, System.Func<File, RotatingFileOutputStreamSupplier.RotationListener, RotatingFileOutputStreamSupplier> fileSupplierStreamCreator ) : base( registry, "csv-reporter", MetricFilter.ALL, rateUnit, durationUnit )
		 {
			  this._locale = locale;
			  this._clock = clock;
			  this._directory = directory;
			  this._fileSupplierStreamCreator = fileSupplierStreamCreator;
			  this._writers = new ConcurrentDictionary<File, CsvRotatableWriter>();
		 }

		 public static Builder ForRegistry( MetricRegistry registry )
		 {
			  return new Builder( registry );
		 }

		 public override void Stop()
		 {
			  base.Stop();
			  _writers.Values.forEach( CsvRotatableWriter.close );
		 }

		 public override void Report( SortedDictionary<string, Gauge> gauges, SortedDictionary<string, Counter> counters, SortedDictionary<string, Histogram> histograms, SortedDictionary<string, Meter> meters, SortedDictionary<string, Timer> timers )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timestamp = java.util.concurrent.TimeUnit.MILLISECONDS.toSeconds(clock.getTime());
			  long timestamp = TimeUnit.MILLISECONDS.toSeconds( _clock.Time );

			  foreach ( KeyValuePair<string, Gauge> entry in gauges.SetOfKeyValuePairs() )
			  {
					ReportGauge( timestamp, entry.Key, entry.Value );
			  }

			  foreach ( KeyValuePair<string, Counter> entry in counters.SetOfKeyValuePairs() )
			  {
					ReportCounter( timestamp, entry.Key, entry.Value );
			  }

			  foreach ( KeyValuePair<string, Histogram> entry in histograms.SetOfKeyValuePairs() )
			  {
					ReportHistogram( timestamp, entry.Key, entry.Value );
			  }

			  foreach ( KeyValuePair<string, Meter> entry in meters.SetOfKeyValuePairs() )
			  {
					ReportMeter( timestamp, entry.Key, entry.Value );
			  }

			  foreach ( KeyValuePair<string, Timer> entry in timers.SetOfKeyValuePairs() )
			  {
					ReportTimer( timestamp, entry.Key, entry.Value );
			  }
		 }

		 private void ReportTimer( long timestamp, string name, Timer timer )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.codahale.metrics.Snapshot snapshot = timer.getSnapshot();
			  Snapshot snapshot = timer.Snapshot;

			  report( timestamp, name, "count,max,mean,min,stddev,p50,p75,p95,p98,p99,p999,mean_rate,m1_rate,m5_rate,m15_rate,rate_unit,duration_unit", "%d,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,calls/%s,%s", timer.Count, convertDuration( snapshot.Max ), convertDuration( snapshot.Mean ), convertDuration( snapshot.Min ), convertDuration( snapshot.StdDev ), convertDuration( snapshot.Median ), convertDuration( snapshot.get75thPercentile() ), convertDuration(snapshot.get95thPercentile()), convertDuration(snapshot.get98thPercentile()), convertDuration(snapshot.get99thPercentile()), convertDuration(snapshot.get999thPercentile()), convertRate(timer.MeanRate), convertRate(timer.OneMinuteRate), convertRate(timer.FiveMinuteRate), convertRate(timer.FifteenMinuteRate), RateUnit, DurationUnit );
		 }

		 private void ReportMeter( long timestamp, string name, Meter meter )
		 {
			  report( timestamp, name, "count,mean_rate,m1_rate,m5_rate,m15_rate,rate_unit", "%d,%f,%f,%f,%f,events/%s", meter.Count, convertRate( meter.MeanRate ), convertRate( meter.OneMinuteRate ), convertRate( meter.FiveMinuteRate ), convertRate( meter.FifteenMinuteRate ), RateUnit );
		 }

		 private void ReportHistogram( long timestamp, string name, Histogram histogram )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.codahale.metrics.Snapshot snapshot = histogram.getSnapshot();
			  Snapshot snapshot = histogram.Snapshot;

			  report( timestamp, name, "count,max,mean,min,stddev,p50,p75,p95,p98,p99,p999", "%d,%d,%f,%d,%f,%f,%f,%f,%f,%f,%f", histogram.Count, snapshot.Max, snapshot.Mean, snapshot.Min, snapshot.StdDev, snapshot.Median, snapshot.get75thPercentile(), snapshot.get95thPercentile(), snapshot.get98thPercentile(), snapshot.get99thPercentile(), snapshot.get999thPercentile() );
		 }

		 private void ReportCounter( long timestamp, string name, Counter counter )
		 {
			  report( timestamp, name, "count", "%d", counter.Count );
		 }

		 private void ReportGauge( long timestamp, string name, Gauge gauge )
		 {
			  report( timestamp, name, "value", "%s", gauge.Value );
		 }

		 private void Report( long timestamp, string name, string header, string line, params object[] values )
		 {
			  File file = new File( _directory, name + ".csv" );
			  CsvRotatableWriter csvRotatableWriter = _writers.computeIfAbsent( file, new RotatingCsvWriterSupplier( header, _fileSupplierStreamCreator, _writers ) );
			  //noinspection SynchronizationOnLocalVariableOrMethodParameter
			  csvRotatableWriter.WriteValues( _locale, timestamp, line, values );
		 }

		 public class Builder
		 {
			  internal readonly MetricRegistry Registry;
			  internal Locale Locale;
			  internal TimeUnit RateUnit;
			  internal TimeUnit DurationUnit;
			  internal Clock Clock;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal System.Func<File, RotatingFileOutputStreamSupplier.RotationListener, RotatingFileOutputStreamSupplier> OutputStreamSupplierFactoryConflict;

			  internal Builder( MetricRegistry registry )
			  {
					this.Registry = registry;
					this.Locale = Locale.Default;
					this.RateUnit = TimeUnit.SECONDS;
					this.DurationUnit = TimeUnit.MILLISECONDS;
					this.Clock = Clock.defaultClock();
			  }

			  public virtual Builder FormatFor( Locale locale )
			  {
					this.Locale = locale;
					return this;
			  }

			  public virtual Builder ConvertRatesTo( TimeUnit rateUnit )
			  {
					this.RateUnit = rateUnit;
					return this;
			  }

			  public virtual Builder ConvertDurationsTo( TimeUnit durationUnit )
			  {
					this.DurationUnit = durationUnit;
					return this;
			  }

			  public virtual Builder OutputStreamSupplierFactory( System.Func<File, RotatingFileOutputStreamSupplier.RotationListener, RotatingFileOutputStreamSupplier> outputStreamSupplierFactory )
			  {
					this.OutputStreamSupplierFactoryConflict = outputStreamSupplierFactory;
					return this;
			  }

			  /// <summary>
			  /// Builds a <seealso cref="RotatableCsvReporter"/> with the given properties, writing {@code .csv} files to the
			  /// given directory.
			  /// </summary>
			  /// <param name="directory"> the directory in which the {@code .csv} files will be created </param>
			  /// <returns> a <seealso cref="RotatableCsvReporter"/> </returns>
			  public virtual RotatableCsvReporter Build( File directory )
			  {
					return new RotatableCsvReporter( Registry, Locale, RateUnit, DurationUnit, Clock, directory, OutputStreamSupplierFactoryConflict );
			  }
		 }

		 private class RotatingCsvWriterSupplier : System.Func<File, CsvRotatableWriter>
		 {
			  internal readonly string Header;
			  internal readonly System.Func<File, RotatingFileOutputStreamSupplier.RotationListener, RotatingFileOutputStreamSupplier> FileSupplierStreamCreator;
			  internal readonly IDictionary<File, CsvRotatableWriter> Writers;

			  internal RotatingCsvWriterSupplier( string header, System.Func<File, RotatingFileOutputStreamSupplier.RotationListener, RotatingFileOutputStreamSupplier> fileSupplierStreamCreator, IDictionary<File, CsvRotatableWriter> writers )
			  {
					this.Header = header;
					this.FileSupplierStreamCreator = fileSupplierStreamCreator;
					this.Writers = writers;
			  }

			  public override CsvRotatableWriter Apply( File file )
			  {
					RotatingFileOutputStreamSupplier outputStreamSupplier = FileSupplierStreamCreator.apply( file, new HeaderWriterRotationListener( this ) );
					PrintWriter printWriter = CreateWriter( outputStreamSupplier.Get() );
					CsvRotatableWriter writer = new CsvRotatableWriter( printWriter, outputStreamSupplier );
					WriteHeader( printWriter, Header );
					return writer;
			  }

			  private class HeaderWriterRotationListener : RotatingFileOutputStreamSupplier.RotationListener
			  {
				  private readonly RotatableCsvReporter.RotatingCsvWriterSupplier _outerInstance;

				  public HeaderWriterRotationListener( RotatableCsvReporter.RotatingCsvWriterSupplier outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }


					public override void RotationCompleted( Stream @out )
					{
						 base.RotationCompleted( @out );
						 using ( PrintWriter writer = CreateWriter( @out ) )
						 {
							  WriteHeader( writer, outerInstance.Header );
						 }
					}
			  }
			  internal static PrintWriter CreateWriter( Stream outputStream )
			  {
					return new PrintWriter( new StreamWriter( outputStream, Encoding.UTF8 ) );
			  }

			  internal static void WriteHeader( PrintWriter printWriter, string header )
			  {
					printWriter.println( "t," + header );
					printWriter.flush();
			  }
		 }

		 private class CsvRotatableWriter
		 {
			  internal readonly PrintWriter PrintWriter;
			  internal readonly RotatingFileOutputStreamSupplier StreamSupplier;

			  internal CsvRotatableWriter( PrintWriter printWriter, RotatingFileOutputStreamSupplier streamSupplier )
			  {
					this.PrintWriter = printWriter;
					this.StreamSupplier = streamSupplier;
			  }

			  internal virtual void Close()
			  {
					IOUtils.closeAllSilently( PrintWriter, StreamSupplier );
			  }

			  internal virtual void WriteValues( Locale locale, long timestamp, string line, object[] values )
			  {
				  lock ( this )
				  {
						StreamSupplier.get();
						PrintWriter.printf( locale, string.format( locale, "%d,%s%n", timestamp, line ), values );
						PrintWriter.flush();
				  }
			  }
		 }
	}

}