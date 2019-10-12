using System;
using System.Collections.Generic;

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
namespace Neo4Net.Logging
{

	using Suppliers = Neo4Net.Function.Suppliers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.FormattedLog.OUTPUT_STREAM_CONVERTER;

	/// <summary>
	/// A <seealso cref="LogProvider"/> implementation that applies a simple formatting to each log message.
	/// </summary>
	public class FormattedLogProvider : AbstractLogProvider<FormattedLog>
	{
		 private static readonly Pattern _packagePattern = Pattern.compile( "(\\w)\\w+\\." );

		 /// <summary>
		 /// A Builder for a <seealso cref="FormattedLogProvider"/>
		 /// </summary>
		 public class Builder
		 {
			  internal bool RenderContext = true;
			  internal ZoneId ZoneId = ZoneOffset.UTC;
			  internal IDictionary<string, Level> Levels = new Dictionary<string, Level>();
			  internal Level DefaultLevel = Level.Info;
			  internal bool AutoFlush = true;

			  internal Builder()
			  {
			  }

			  /// <summary>
			  /// Disable rendering of the context (the class name or log name) in each output line.
			  /// </summary>
			  /// <returns> this builder </returns>
			  public virtual Builder WithoutRenderingContext()
			  {
					this.RenderContext = false;
					return this;
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
			  /// <param name="zoneId"> to use </param>
			  public virtual Builder WithZoneId( ZoneId zoneId )
			  {
					this.ZoneId = zoneId;
					return this;
			  }

			  /// <summary>
			  /// Set the zoneId from timestamp for datestamps in the log
			  /// </summary>
			  /// <param name="timezone"> to use </param>
			  /// <returns> this builder </returns>
			  /// @deprecated use <seealso cref="withZoneId(ZoneId)"/> 
			  [Obsolete("use <seealso cref=\"withZoneId(ZoneId)\"/>")]
			  public virtual Builder WithTimeZone( TimeZone timezone )
			  {
					return WithZoneId( timezone.toZoneId() );
			  }

			  /// <summary>
			  /// Use the specified log <seealso cref="Level"/> for all <seealso cref="Log"/>s by default.
			  /// </summary>
			  /// <param name="level"> the log level to use as a default </param>
			  /// <returns> this builder </returns>
			  public virtual Builder WithDefaultLogLevel( Level level )
			  {
					this.DefaultLevel = level;
					return this;
			  }

			  /// <summary>
			  /// Use the specified log <seealso cref="Level"/> for any <seealso cref="Log"/>s that match the specified context. Any <seealso cref="Log"/> context that
			  /// starts with the specified string will have its level set. For example, setting the level for the context {@code org.neo4j}
			  /// would result in that level being applied to <seealso cref="Log"/>s with the context {@code org.neo4j.Foo}, {@code org.neo4j.foo.Bar}, etc.
			  /// </summary>
			  /// <param name="context"> the context of the Logs to set the level of, matching any Log context starting with this string </param>
			  /// <param name="level"> the log level to apply </param>
			  /// <returns> this builder </returns>
			  public virtual Builder WithLogLevel( string context, Level level )
			  {
					this.Levels[context] = level;
					return this;
			  }

			  /// <summary>
			  /// Set the log level for many contexts - equivalent to calling <seealso cref="withLogLevel(string, Level)"/>
			  /// for every entry in the provided map.
			  /// </summary>
			  /// <param name="levels"> a map containing paris of context and level </param>
			  /// <returns> this builder </returns>
			  public virtual Builder WithLogLevels( IDictionary<string, Level> levels )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					this.Levels.putAll( levels );
					return this;
			  }

			  /// <summary>
			  /// Disable auto flushing.
			  /// </summary>
			  /// <returns> this builder </returns>
			  public virtual Builder WithoutAutoFlush()
			  {
					this.AutoFlush = false;
					return this;
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to an <seealso cref="System.IO.Stream_Output"/>.
			  /// </summary>
			  /// <param name="out"> An <seealso cref="System.IO.Stream_Output"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified OutputStream </returns>
			  public virtual FormattedLogProvider ToOutputStream( Stream @out )
			  {
					return ToPrintWriter( Suppliers.singleton( OUTPUT_STREAM_CONVERTER.apply( @out ) ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to <seealso cref="System.IO.Stream_Output"/>s obtained from the specified
			  /// <seealso cref="Supplier"/>. The OutputStream is obtained from the Supplier before every log message is written.
			  /// </summary>
			  /// <param name="outSupplier"> A supplier for an output stream to write to </param>
			  /// <returns> A <seealso cref="FormattedLogProvider"/> instance </returns>
			  public virtual FormattedLogProvider ToOutputStream( System.Func<Stream> outSupplier )
			  {
					return ToPrintWriter( Suppliers.adapted( outSupplier, OUTPUT_STREAM_CONVERTER ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to a <seealso cref="Writer"/>.
			  /// </summary>
			  /// <param name="writer"> A <seealso cref="Writer"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified Writer </returns>
			  public virtual FormattedLogProvider ToWriter( Writer writer )
			  {
					return ToPrintWriter( new PrintWriter( writer ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to a <seealso cref="PrintWriter"/>.
			  /// </summary>
			  /// <param name="writer"> A <seealso cref="PrintWriter"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified PrintWriter </returns>
			  public virtual FormattedLogProvider ToPrintWriter( PrintWriter writer )
			  {
					return ToPrintWriter( Suppliers.singleton( writer ) );
			  }

			  /// <summary>
			  /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to <seealso cref="PrintWriter"/>s obtained from the specified
			  /// <seealso cref="Supplier"/>. The PrintWriter is obtained from the Supplier before every log message is written.
			  /// </summary>
			  /// <param name="writerSupplier"> A supplier for a <seealso cref="PrintWriter"/> to write to </param>
			  /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified PrintWriter </returns>
			  public virtual FormattedLogProvider ToPrintWriter( System.Func<PrintWriter> writerSupplier )
			  {
					return new FormattedLogProvider( writerSupplier, ZoneId, RenderContext, Levels, DefaultLevel, AutoFlush );
			  }
		 }

		 private readonly System.Func<PrintWriter> _writerSupplier;
		 private readonly ZoneId _zoneId;
		 private readonly bool _renderContext;
		 private readonly IDictionary<string, Level> _levels;
		 private readonly Level _defaultLevel;
		 private readonly bool _autoFlush;

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLogProvider"/> which will not render the context (the class name or log name) in each output line.
		 /// Use <seealso cref="Builder.toOutputStream"/> to complete.
		 /// </summary>
		 /// <returns> a builder for a <seealso cref="FormattedLogProvider"/> </returns>
		 public static Builder WithoutRenderingContext()
		 {
			  return ( new Builder() ).WithoutRenderingContext();
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLogProvider"/> with UTC timezone for datestamps in the log
		 /// </summary>
		 /// <returns> a builder for a <seealso cref="FormattedLogProvider"/> </returns>
		 public static Builder WithUTCTimeZone()
		 {
			  return ( new Builder() ).WithUTCZoneId();
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLogProvider"/> with the specified zoneId for datestamps in the log
		 /// </summary>
		 /// <returns> a builder for a <seealso cref="FormattedLogProvider"/> </returns>
		 /// <param name="zoneId"> to use </param>
		 public static Builder WithZoneId( ZoneId zoneId )
		 {
			  return ( new Builder() ).WithZoneId(zoneId);
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLogProvider"/> with the specified zoneId from timezone for datestamps in the log
		 /// </summary>
		 /// <param name="timeZone"> to use </param>
		 /// <returns> a builder for a <seealso cref="FormattedLogProvider"/> </returns>
		 /// @deprecated use <seealso cref="withZoneId(ZoneId)"/> 
		 [Obsolete("use <seealso cref=\"withZoneId(ZoneId)\"/>")]
		 public static Builder WithTimeZone( TimeZone timeZone )
		 {
			  return ( new Builder() ).WithZoneId(timeZone.toZoneId());
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLogProvider"/> with the specified log <seealso cref="Level"/> for all <seealso cref="Log"/>s by default.
		 /// Use <seealso cref="Builder.toOutputStream"/> to complete.
		 /// </summary>
		 /// <param name="level"> the log level to use as a default </param>
		 /// <returns> a builder for a <seealso cref="FormattedLogProvider"/> </returns>
		 public static Builder WithDefaultLogLevel( Level level )
		 {
			  return ( new Builder() ).WithDefaultLogLevel(level);
		 }

		 /// <summary>
		 /// Start creating a <seealso cref="FormattedLogProvider"/> without auto flushing.
		 /// Use <seealso cref="Builder.toOutputStream"/> to complete.
		 /// </summary>
		 /// <returns> a builder for a <seealso cref="FormattedLogProvider"/> </returns>
		 public static Builder WithoutAutoFlush()
		 {
			  return ( new Builder() ).WithoutAutoFlush();
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to an <seealso cref="System.IO.Stream_Output"/>.
		 /// </summary>
		 /// <param name="out"> An <seealso cref="System.IO.Stream_Output"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified OutputStream </returns>
		 public static FormattedLogProvider ToOutputStream( Stream @out )
		 {
			  return ( new Builder() ).ToOutputStream(@out);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to <seealso cref="System.IO.Stream_Output"/>s obtained from the specified
		 /// <seealso cref="Supplier"/>. The OutputStream is obtained from the Supplier before every log message is written.
		 /// </summary>
		 /// <param name="outSupplier"> A supplier for an output stream to write to </param>
		 /// <returns> A <seealso cref="FormattedLogProvider"/> instance </returns>
		 public static FormattedLogProvider ToOutputStream( System.Func<Stream> outSupplier )
		 {
			  return ( new Builder() ).ToOutputStream(outSupplier);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to a <seealso cref="Writer"/>.
		 /// </summary>
		 /// <param name="writer"> A <seealso cref="Writer"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified Writer </returns>
		 public static FormattedLogProvider ToWriter( Writer writer )
		 {
			  return ( new Builder() ).ToWriter(writer);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to a <seealso cref="PrintWriter"/>.
		 /// </summary>
		 /// <param name="writer"> A <seealso cref="PrintWriter"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified PrintWriter </returns>
		 public static FormattedLogProvider ToPrintWriter( PrintWriter writer )
		 {
			  return ( new Builder() ).ToPrintWriter(writer);
		 }

		 /// <summary>
		 /// Creates a <seealso cref="FormattedLogProvider"/> instance that writes messages to <seealso cref="PrintWriter"/>s obtained from the specified
		 /// <seealso cref="Supplier"/>. The PrintWriter is obtained from the Supplier before every log message is written.
		 /// </summary>
		 /// <param name="writerSupplier"> A supplier for a <seealso cref="PrintWriter"/> to write to </param>
		 /// <returns> A <seealso cref="FormattedLogProvider"/> instance that writes to the specified PrintWriter </returns>
		 public static FormattedLogProvider ToPrintWriter( System.Func<PrintWriter> writerSupplier )
		 {
			  return ( new Builder() ).ToPrintWriter(writerSupplier);
		 }

		 internal FormattedLogProvider( System.Func<PrintWriter> writerSupplier, ZoneId zoneId, bool renderContext, IDictionary<string, Level> levels, Level defaultLevel, bool autoFlush )
		 {
			  this._writerSupplier = writerSupplier;
			  this._zoneId = zoneId;
			  this._renderContext = renderContext;
			  this._levels = new Dictionary<string, Level>( levels );
			  this._defaultLevel = defaultLevel;
			  this._autoFlush = autoFlush;
		 }

		 protected internal override FormattedLog BuildLog( Type loggingClass )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  string className = loggingClass.FullName;
			  string shortenedClassName = _packagePattern.matcher( className ).replaceAll( "$1." );
			  return BuildLog( shortenedClassName, LevelForContext( className ) );
		 }

		 protected internal override FormattedLog BuildLog( string name )
		 {
			  return BuildLog( name, LevelForContext( name ) );
		 }

		 private FormattedLog BuildLog( string context, Level level )
		 {
			  return new FormattedLog( _writerSupplier, _zoneId, this, _renderContext ? context : null, level, _autoFlush );
		 }

		 private Level LevelForContext( string context )
		 {
			  foreach ( KeyValuePair<string, Level> entry in _levels.SetOfKeyValuePairs() )
			  {
					if ( context.StartsWith( entry.Key, StringComparison.Ordinal ) )
					{
						 return entry.Value;
					}
			  }
			  return _defaultLevel;
		 }
	}

}