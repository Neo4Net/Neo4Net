using System;

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
namespace Neo4Net.Consistency
{

	using Suppliers = Neo4Net.Functions.Suppliers;
	using AbstractPrintWriterLogger = Neo4Net.Logging.AbstractPrintWriterLogger;
	using Logger = Neo4Net.Logging.Logger;

	public class ConsistencyReportLogger : AbstractPrintWriterLogger
	{
		 private readonly string _prefix;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ConsistencyReportLogger(@Nonnull Supplier<java.io.PrintWriter> writerSupplier, @Nonnull Object lock, String prefix, boolean autoFlush)
		 public ConsistencyReportLogger( Supplier<PrintWriter> writerSupplier, object @lock, string prefix, bool autoFlush ) : base( writerSupplier, @lock, autoFlush )
		 {
			  this._prefix = prefix;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override protected void writeLog(@Nonnull PrintWriter out, @Nonnull String message)
		 protected internal override void WriteLog( PrintWriter @out, string message )
		 {
			  @out.write( _prefix );
			  @out.write( ": " );
			  @out.write( message );
			  @out.println();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override protected void writeLog(@Nonnull PrintWriter out, @Nonnull String message, @Nonnull Throwable throwable)
		 protected internal override void WriteLog( PrintWriter @out, string message, Exception throwable )
		 {
			  @out.write( _prefix );
			  @out.write( ": " );
			  @out.write( message );
			  @out.write( ' ' );
			  @out.write( throwable.Message );
			  @out.println();
			  throwable.printStackTrace( @out );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override protected Neo4Net.logging.Logger getBulkLogger(@Nonnull PrintWriter out, @Nonnull Object lock)
		 protected internal override Logger GetBulkLogger( PrintWriter @out, object @lock )
		 {
			  return new ConsistencyReportLogger( Suppliers.singleton( @out ), @lock, _prefix, false );
		 }
	}

}