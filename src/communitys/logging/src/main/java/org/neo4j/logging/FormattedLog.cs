using System;
using System.IO;
using System.Text;

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
namespace Neo4Net.Logging
{

	using Suppliers = Neo4Net.Functions.Suppliers;

	/// <summary>
	/// A <seealso cref="Log"/> implementation that applies a simple formatting to each log message.
	/// </summary>
	public class FormattedLog : AbstractLog
	{
		 internal static readonly System.Func<Stream, PrintWriter> OutputStreamConverter = outputStream => new PrintWriter( new StreamWriter( outputStream, Encoding.UTF8 ) );

		 /// <summary>
		 /// A Builder for a <seealso cref="FormattedLog"/>
		 /// </summary>
		 public class Builder
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 Lock = this;
				 DateTimeFormatterSupplier = () => FormattedLogger.DefaultCurrentDateTime.apply(ZoneId);
			 }

			  internal ZoneId ZoneId = ZoneOffset.UTC;
			  internal object Lock;
			  internal string Category;
			  internal Level Level = Level.Info;
			  internal bool AutoFlush = true;
			  internal DateTimeFormatter DateTimeFormatter = FormattedLogger.DateTimeFormatter;
			  internal System.Func<ZonedDateTime> DateTimeFormatterSupplier;

			  internal Builder()
			  {
				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
			  }

			  /// <summary>
			  /// Set the zoneId for datestamps in the log
			  /// </summary>
			  /// <returns> this builder </returns>
			  public virtual Builder WithUTCZoneId()
			  {
					return WithZoneId( ZoneOffset.UTC );
			  }

			  /// <summary>
			  /// Set the zoneId for datestamps in the log
			  /// </summary>
			  /// <returns> this builder </returns>
			  /// <param name="timezone"> to use </param>
			  /// @deprecated use <seealso cref="withZoneId(ZoneId)"/> 
			  [Obsolete("use <seealso cref=\"withZoneId(ZoneId)\"/>")]
			  public virtual Builder WithTimeZone( TimeZone timezone )
			  {
					return this.WithZoneId( timezone.toZoneId() );
			  }

			  /// <summary>
			  /// Set the zoneId for datestamps in the log
			  /// </summary>
			  /// <param name="zoneId"> to use </param>
			  /// <returns> this builder </returns>
			  public virtual Builder WithZoneId( ZoneId zoneId )
			  {
					this.ZoneId = zoneId;
					return this;
			  }

			  /// <summary>
			  /// Set the dateFormat for datestamps in the log
			  /// </summary>
			  /// <param name="dateTimeFormatter"> the dateFormat to use for datestamps </param>
			  /// <returns> this builder </returns>
			  public virtual Builder WithDateTimeFormatter( DateTimeFormatter dateTimeFormatter )
			  {
					this.DateTimeFormatter = dateTimeFormatter;
					return this;
			  }

			  /// <summary>
			  /// Use the specified object to synchronize on.
			  /// </summary>
			  /// <param name="lock"> the object to synchronize on </param>
			  /// <returns> this builder </returns>
			  public virtual Builder UsingLock( object @lock )
			  {
					this.Lock = @lock;
					return this;
			  }

			  /// <summary>
			  /// Include the specified category in each output log line.
			  /// </summary>
			  /// <param name="category"> the category to include ing each output line </param>
			  /// <returns> this builder </returns>
			  public virtual Builder WithCategory( string category )
			  {
					this.Category = category;
					return this;
			  }

			  /// <summary>
			  /// Use the specified log <seealso cref="Level"/> as a default.
			  /// </summary>
			  /// <param name="level"> the log level to use as a default </param>
			  /// <returns> this builder </returns>
			  public virtual Builder WithLogLevel( Level level )
			  {
					this.Level = level;
					return this;
			  }

			  /// <summary>
			  /// Use the specified function
			  /// </summary>
			  /// <param name="zonedDateTimeSupplier"> the log level to use as a default </param>
			  /// <returns> this builder </returns>
			  internal virtual Builder WithTimeSupplier( System.Func<ZonedDateTime> zonedDateTimeSupplier )
			  {
					this.DateTimeFormatterSupplier = zonedDateTimeSupplier;
					return this;
			  }

			  /// <summary>
			  /// Disable auto flushing.
			  /// </summary>
			  /// <returns> this builder </returns>
			  public virtual Builder WithoutAutoFlush()
			  {
					AutoFlush = false;
					return this;
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to an <seealso cref="System.IO.Stream_Output"/>.
			  /// </summary>
			  /// <param name="out"> An <seealso cref="System.IO.Stream_Output"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified OutputStream </returns>
			  public virtual FormattedLog ToOutputStream( Stream @out )
			  {
					return ToPrintWriter( Suppliers.singleton( OutputStreamConverter.apply( @out ) ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to <seealso cref="System.IO.Stream_Output"/>s obtained from the specified
			  /// <seealso cref="Supplier"/>. The OutputStream is obtained from the Supplier before every log message is written.
			  /// </summary>
			  /// <param name="outSupplier"> A supplier for an output stream to write to </param>
			  /// <returns> A <seealso cref="FormattedLog"/> instance </returns>
			  public virtual FormattedLog ToOutputStream( System.Func<Stream> outSupplier )
			  {
					return ToPrintWriter( Suppliers.adapted( outSupplier, OutputStreamConverter ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to a <seealso cref="Writer"/>.
			  /// </summary>
			  /// <param name="writer"> A <seealso cref="Writer"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified Writer </returns>
			  public virtual FormattedLog ToWriter( Writer writer )
			  {
					return ToPrintWriter( new PrintWriter( writer ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to a <seealso cref="PrintWriter"/>.
			  /// </summary>
			  /// <param name="writer"> A <seealso cref="PrintWriter"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified PrintWriter </returns>
			  public virtual FormattedLog ToPrintWriter( PrintWriter writer )
			  {
					return ToPrintWriter( Suppliers.singleton( writer ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to <seealso cref="PrintWriter"/>s obtained from the specified
			  /// <seealso cref="Supplier"/>. The PrintWriter is obtained from the Supplier before every log message is written.
			  /// </summary>
			  /// <param name="writerSupplier"> A supplier for a <seealso cref="PrintWriter"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified PrintWriter </returns>
			  public virtual FormattedLog ToPrintWriter( System.Func<PrintWriter> writerSupplier )
			  {
					return new FormattedLog( writerSupplier, ZoneId, Lock, Category, Level, AutoFlush, DateTimeFormatter, DateTimeFormatterSupplier );
			  }
		 }

		 private readonly System.Func<PrintWriter> _writerSupplier;
		 internal readonly ZoneId ZoneId;
		 internal readonly object Lock;
		 private readonly string _category;
		 private readonly AtomicReference<Level> _levelRef;
		 internal readonly bool AutoFlush;
		 private readonly Logger _debugLogger;
		 private readonly Logger _infoLogger;
		 private readonly Logger _warnLogger;
		 private readonly Logger _errorLogger;

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLog"/> with UTC timezone for datestamps in the log
		 /// </summary>
		 /// <returns> a builder for a <seealso cref="FormattedLog"/> </returns>
		 public static Builder WithUTCTimeZone()
		 {
			  return ( new Builder() ).WithUTCZoneId();
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLog"/> with the specified zoneId from timezone for datestamps in the log
		 /// </summary>
		 /// <param name="timezone"> to use </param>
		 /// <returns> a builder for a <seealso cref="FormattedLog"/> </returns>
		 /// @deprecated use <seealso cref="withZoneId(ZoneId)"/> 
		 [Obsolete("use <seealso cref=\"withZoneId(ZoneId)\"/>")]
		 public static Builder WithTimeZone( TimeZone timezone )
		 {
			  return ( new Builder() ).WithZoneId(timezone.toZoneId());
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLog"/> with the specified zoneId for datestamps in the log
		 /// </summary>
		 /// <param name="zoneId"> to use </param>
		 /// <returns> a builder for a <seealso cref="FormattedLog"/> </returns>
		 public static Builder WithZoneId( ZoneId zoneId )
		 {
			  return ( new Builder() ).WithZoneId(zoneId);
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLog"/> using the specified object to synchronize on.
		 /// Use <seealso cref="Builder.toOutputStream"/> to complete.
		 /// </summary>
		 /// <param name="lock"> the object to synchronize on </param>
		 /// <returns> a builder for a <seealso cref="FormattedLog"/> </returns>
		 public static Builder UsingLock( object @lock )
		 {
			  return ( new Builder() ).UsingLock(@lock);
		 }

		 /// <summary>
		 /// Include the specified category in each output log line
		 /// </summary>
		 /// <param name="category"> the category to include ing each output line </param>
		 /// <returns> a builder for a <seealso cref="FormattedLog"/> </returns>
		 public static Builder WithCategory( string category )
		 {
			  return ( new Builder() ).WithCategory(category);
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLog"/> with the specified log <seealso cref="Level"/> as a default.
		 /// Use <seealso cref="Builder.toOutputStream"/> to complete.
		 /// </summary>
		 /// <param name="level"> the log level to use as a default </param>
		 /// <returns> a builder for a <seealso cref="FormattedLog"/> </returns>
		 public static Builder WithLogLevel( Level level )
		 {
			  return ( new Builder() ).WithLogLevel(level);
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLog"/> without auto flushing.
		 /// Use <seealso cref="Builder.toOutputStream"/> to complete.
		 /// </summary>
		 /// <returns> a builder for a <seealso cref="FormattedLog"/> </returns>
		 public static Builder WithoutAutoFlush()
		 {
			  return ( new Builder() ).WithoutAutoFlush();
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to an <seealso cref="System.IO.Stream_Output"/>.
		 /// </summary>
		 /// <param name="out"> An <seealso cref="System.IO.Stream_Output"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified OutputStream </returns>
		 public static FormattedLog ToOutputStream( Stream @out )
		 {
			  return ( new Builder() ).ToOutputStream(@out);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to <seealso cref="System.IO.Stream_Output"/>s obtained from the specified
		 /// <seealso cref="Supplier"/>. The OutputStream is obtained from the Supplier before every log message is written.
		 /// </summary>
		 /// <param name="outSupplier"> A supplier for an output stream to write to </param>
		 /// <returns> A <seealso cref="FormattedLog"/> instance </returns>
		 public static FormattedLog ToOutputStream( System.Func<Stream> outSupplier )
		 {
			  return ( new Builder() ).ToOutputStream(outSupplier);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to a <seealso cref="Writer"/>.
		 /// </summary>
		 /// <param name="writer"> A <seealso cref="Writer"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified Writer </returns>
		 public static FormattedLog ToWriter( Writer writer )
		 {
			  return ( new Builder() ).ToWriter(writer);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to a <seealso cref="PrintWriter"/>.
		 /// </summary>
		 /// <param name="writer"> A <seealso cref="PrintWriter"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified PrintWriter </returns>
		 public static FormattedLog ToPrintWriter( PrintWriter writer )
		 {
			  return ( new Builder() ).ToPrintWriter(writer);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLog"/> instance that writes messages to <seealso cref="PrintWriter"/>s obtained from the specified
		 /// <seealso cref="Supplier"/>. The PrintWriter is obtained from the Supplier before every log message is written.
		 /// </summary>
		 /// <param name="writerSupplier"> A supplier for a <seealso cref="PrintWriter"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLog"/> instance that writes to the specified PrintWriter </returns>
		 public static FormattedLog ToPrintWriter( System.Func<PrintWriter> writerSupplier )
		 {
			  return ( new Builder() ).ToPrintWriter(writerSupplier);
		 }

		 protected internal FormattedLog( System.Func<PrintWriter> writerSupplier, ZoneId zoneId, object maybeLock, string category, Level level, bool autoFlush ) : this( writerSupplier, zoneId, maybeLock, category, level, autoFlush, FormattedLogger.DateTimeFormatter, () -> FormattedLogger.DefaultCurrentDateTime.apply(zoneId) )
		 {
		 }

		 protected internal FormattedLog( System.Func<PrintWriter> writerSupplier, ZoneId zoneId, object maybeLock, string category, Level level, bool autoFlush, DateTimeFormatter dateTimeFormatter, System.Func<ZonedDateTime> dateTimeSupplier )
		 {
			  this._writerSupplier = writerSupplier;
			  this.ZoneId = zoneId;
			  this.Lock = ( maybeLock != null ) ? maybeLock : this;
			  this._category = category;
			  this._levelRef = new AtomicReference<Level>( level );
			  this.AutoFlush = autoFlush;

			  string debugPrefix = ( !string.ReferenceEquals( category, null ) && category.Length > 0 ) ? "DEBUG [" + category + "]" : "DEBUG";
			  string infoPrefix = ( !string.ReferenceEquals( category, null ) && category.Length > 0 ) ? "INFO [" + category + "]" : "INFO ";
			  string warnPrefix = ( !string.ReferenceEquals( category, null ) && category.Length > 0 ) ? "WARN [" + category + "]" : "WARN ";
			  string errorPrefix = ( !string.ReferenceEquals( category, null ) && category.Length > 0 ) ? "ERROR [" + category + "]" : "ERROR";

			  this._debugLogger = new FormattedLogger( this, writerSupplier, debugPrefix, dateTimeFormatter, dateTimeSupplier );
			  this._infoLogger = new FormattedLogger( this, writerSupplier, infoPrefix, dateTimeFormatter, dateTimeSupplier );
			  this._warnLogger = new FormattedLogger( this, writerSupplier, warnPrefix, dateTimeFormatter, dateTimeSupplier );
			  this._errorLogger = new FormattedLogger( this, writerSupplier, errorPrefix, dateTimeFormatter, dateTimeSupplier );
		 }

		 /// <summary>
		 /// Get the current <seealso cref="Level"/> that logging is enabled at
		 /// </summary>
		 /// <returns> the current level that logging is enabled at </returns>
		 public virtual Level Level
		 {
			 get
			 {
				  return _levelRef.get();
			 }
		 }

		 /// <summary>
		 /// Set the <seealso cref="Level"/> that logging should be enabled at
		 /// </summary>
		 /// <param name="level"> the new logging level </param>
		 /// <returns> the previous logging level </returns>
		 public virtual Level setLevel( Level level )
		 {
			  return _levelRef.getAndSet( level );
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  return Level.Debug.compareTo( _levelRef.get() ) >= 0;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger debugLogger()
		 public override Logger DebugLogger()
		 {
			  return DebugEnabled ? this._debugLogger : NullLogger.Instance;
		 }

		 /// <returns> true if the current log level enables info logging </returns>
		 public virtual bool InfoEnabled
		 {
			 get
			 {
				  return Level.Info.compareTo( _levelRef.get() ) >= 0;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger infoLogger()
		 public override Logger InfoLogger()
		 {
			  return InfoEnabled ? this._infoLogger : NullLogger.Instance;
		 }

		 /// <returns> true if the current log level enables warn logging </returns>
		 public virtual bool WarnEnabled
		 {
			 get
			 {
				  return Level.Warn.compareTo( _levelRef.get() ) >= 0;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger warnLogger()
		 public override Logger WarnLogger()
		 {
			  return WarnEnabled ? this._warnLogger : NullLogger.Instance;
		 }

		 /// <returns> true if the current log level enables error logging </returns>
		 public virtual bool ErrorEnabled
		 {
			 get
			 {
				  return Level.Error.compareTo( _levelRef.get() ) >= 0;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger errorLogger()
		 public override Logger ErrorLogger()
		 {
			  return ErrorEnabled ? this._errorLogger : NullLogger.Instance;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Log> consumer)
		 public override void Bulk( Consumer<Log> consumer )
		 {
			  PrintWriter writer;
			  lock ( Lock )
			  {
					writer = _writerSupplier.get();
					consumer.accept( new FormattedLog( Suppliers.singleton( writer ), ZoneId, Lock, _category, _levelRef.get(), false ) );
			  }
			  if ( AutoFlush )
			  {
					writer.flush();
			  }
		 }

	}

}