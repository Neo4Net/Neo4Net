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
	using AbstractLog = Neo4Net.Logging.AbstractLog;
	using Log = Neo4Net.Logging.Log;
	using Logger = Neo4Net.Logging.Logger;
	using NullLogger = Neo4Net.Logging.NullLogger;

	public class ConsistencyReportLog : AbstractLog
	{
		 private readonly System.Func<PrintWriter> _writerSupplier;
		 private readonly object @lock;
		 private readonly Logger _infoLogger;
		 private readonly Logger _warnLogger;
		 private readonly Logger _errorLogger;

		 public ConsistencyReportLog( System.Func<PrintWriter> writerSupplier ) : this( writerSupplier, null, true )
		 {
		 }

		 private ConsistencyReportLog( System.Func<PrintWriter> writerSupplier, object maybeLock, bool autoFlush )
		 {
			  this._writerSupplier = writerSupplier;
			  this.@lock = ( maybeLock != null ) ? maybeLock : this;
			  _infoLogger = new ConsistencyReportLogger( writerSupplier, @lock, "INFO ", autoFlush );
			  _warnLogger = new ConsistencyReportLogger( writerSupplier, @lock, "WARN ", autoFlush );
			  _errorLogger = new ConsistencyReportLogger( writerSupplier, @lock, "ERROR", autoFlush );
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger debugLogger()
		 public override Logger DebugLogger()
		 {
			  return NullLogger.Instance;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger infoLogger()
		 public override Logger InfoLogger()
		 {
			  return _infoLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger warnLogger()
		 public override Logger WarnLogger()
		 {
			  return _warnLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger errorLogger()
		 public override Logger ErrorLogger()
		 {
			  return _errorLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<org.Neo4Net.logging.Log> consumer)
		 public override void Bulk( Consumer<Log> consumer )
		 {
			  PrintWriter writer;
			  lock ( this )
			  {
					writer = _writerSupplier.get();
					consumer.accept( new ConsistencyReportLog( Suppliers.singleton( writer ), @lock, false ) );
			  }
		 }
	}

}