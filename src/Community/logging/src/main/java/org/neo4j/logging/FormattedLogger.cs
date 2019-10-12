using System;

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

	internal class FormattedLogger : AbstractPrintWriterLogger
	{
		 internal static readonly DateTimeFormatter DateTimeFormatter = DateTimeFormatter.ofPattern( "yyyy-MM-dd HH:mm:ss.SSSZ" );
		 internal static readonly System.Func<ZoneId, ZonedDateTime> DefaultCurrentDateTime = zoneId => ZonedDateTime.now().withZoneSameInstant(zoneId);
		 private FormattedLog _formattedLog;
		 private readonly string _prefix;
		 private readonly DateTimeFormatter _dateTimeFormatter;
		 private System.Func<ZonedDateTime> _supplier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: FormattedLogger(FormattedLog formattedLog, @Nonnull Supplier<java.io.PrintWriter> writerSupplier, @Nonnull String prefix, java.time.format.DateTimeFormatter dateTimeFormatter, java.util.function.Supplier<java.time.ZonedDateTime> zonedDateTimeSupplier)
		 internal FormattedLogger( FormattedLog formattedLog, Supplier<PrintWriter> writerSupplier, string prefix, DateTimeFormatter dateTimeFormatter, System.Func<ZonedDateTime> zonedDateTimeSupplier ) : base( writerSupplier, formattedLog.Lock, formattedLog.AutoFlush )
		 {

			  this._formattedLog = formattedLog;
			  this._prefix = prefix;
			  this._dateTimeFormatter = dateTimeFormatter;
			  this._supplier = zonedDateTimeSupplier;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override protected void writeLog(@Nonnull PrintWriter out, @Nonnull String message)
		 protected internal override void WriteLog( PrintWriter @out, string message )
		 {
			  LineStart( @out );
			  @out.write( message );
			  @out.println();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override protected void writeLog(@Nonnull PrintWriter out, @Nonnull String message, @Nonnull Throwable throwable)
		 protected internal override void WriteLog( PrintWriter @out, string message, Exception throwable )
		 {
			  LineStart( @out );
			  @out.write( message );
			  if ( throwable.Message != null )
			  {
					@out.write( ' ' );
					@out.write( throwable.Message );
			  }
			  @out.println();
			  throwable.printStackTrace( @out );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override protected Logger getBulkLogger(@Nonnull PrintWriter out, @Nonnull Object lock)
		 protected internal override Logger GetBulkLogger( PrintWriter @out, object @lock )
		 {
			  return new FormattedLogger( _formattedLog, Suppliers.singleton( @out ), _prefix, DateTimeFormatter, () => DefaultCurrentDateTime.apply(_formattedLog.zoneId) );
		 }

		 private void LineStart( PrintWriter @out )
		 {
			  @out.write( Time() );
			  @out.write( ' ' );
			  @out.write( _prefix );
			  @out.write( ' ' );
		 }

		 private string Time()
		 {
			  return _dateTimeFormatter.format( _supplier.get() );
		 }
	}

}