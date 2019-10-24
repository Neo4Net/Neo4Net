using System.Collections.Generic;

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
namespace Neo4Net.Internal.Collector
{

	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Admin = Neo4Net.Procedure.Admin;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Mode = Neo4Net.Procedure.Mode;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class DataCollectorProcedures
	public class DataCollectorProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public DataCollector dataCollector;
		 public DataCollector DataCollector;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.kernel.api.KernelTransaction transaction;
		 public KernelTransaction Transaction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Retrieve statistical data about the current database. Valid sections are '" + Sections.GRAPH_COUNTS + "', '" + Sections.TOKENS + "', '" + Sections.QUERIES + "', '" + Sections.META + "'") @Procedure(name = "db.stats.retrieve", mode = org.Neo4Net.procedure.Mode.READ) public java.util.stream.Stream<RetrieveResult> retrieve(@Name(value = "section") String section, @Name(value = "config", defaultValue = "") java.util.Map<String, Object> config) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException, org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException, org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Retrieve statistical data about the current database. Valid sections are '" + Sections.GRAPH_COUNTS + "', '" + Sections.TOKENS + "', '" + Sections.QUERIES + "', '" + Sections.META + "'"), Procedure(name : "db.stats.retrieve", mode : Neo4Net.Procedure.Mode.READ)]
		 public virtual Stream<RetrieveResult> Retrieve( string section, IDictionary<string, object> config )
		 {
			  string upperSection = section.ToUpper();
			  switch ( upperSection )
			  {
			  case Sections.GRAPH_COUNTS:
					return GraphCountsSection.Retrieve( DataCollector.kernel, Anonymizer.PLAIN_TEXT );

			  case Sections.TOKENS:
					return TokensSection.Retrieve( DataCollector.kernel );

			  case Sections.META:
					return MetaSection.Retrieve( null, DataCollector.kernel, DataCollector.queryCollector.numSilentQueryDrops() );

			  case Sections.QUERIES:
					return QueriesSection.retrieve( DataCollector.queryCollector.Data, new PlainText( DataCollector.valueMapper ), RetrieveConfig.Of( config ).MaxInvocations );

			  default:
					throw Sections.UnknownSectionException( section );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Retrieve all available statistical data about the current database, in an anonymized form.") @Procedure(name = "db.stats.retrieveAllAnonymized", mode = org.Neo4Net.procedure.Mode.READ) public java.util.stream.Stream<RetrieveResult> retrieveAllAnonymized(@Name(value = "graphToken") String graphToken, @Name(value = "config", defaultValue = "") java.util.Map<String, Object> config) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException, org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Retrieve all available statistical data about the current database, in an anonymized form."), Procedure(name : "db.stats.retrieveAllAnonymized", mode : Neo4Net.Procedure.Mode.READ)]
		 public virtual Stream<RetrieveResult> RetrieveAllAnonymized( string graphToken, IDictionary<string, object> config )
		 {
			  if ( string.ReferenceEquals( graphToken, null ) || graphToken.Equals( "" ) )
			  {
					throw new InvalidArgumentsException( "Graph token must be a non-empty string" );
			  }

			  return Stream.of( MetaSection.Retrieve( graphToken, DataCollector.kernel, DataCollector.queryCollector.numSilentQueryDrops() ), GraphCountsSection.Retrieve(DataCollector.kernel, Anonymizer.IDS), QueriesSection.retrieve(DataCollector.queryCollector.Data, new IdAnonymizer(Transaction.tokenRead()), RetrieveConfig.Of(config).MaxInvocations) ).flatMap(x => x);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Retrieve the status of all available collector daemons, for this database.") @Procedure(name = "db.stats.status", mode = org.Neo4Net.procedure.Mode.READ) public java.util.stream.Stream<StatusResult> status()
		 [Description("Retrieve the status of all available collector daemons, for this database."), Procedure(name : "db.stats.status", mode : Neo4Net.Procedure.Mode.READ)]
		 public virtual Stream<StatusResult> Status()
		 {
			  CollectorStateMachine.Status status = DataCollector.queryCollector.status();
			  return Stream.of( new StatusResult( Sections.QUERIES, status.Message, Collections.emptyMap() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Start data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'") @Procedure(name = "db.stats.collect", mode = org.Neo4Net.procedure.Mode.READ) public java.util.stream.Stream<ActionResult> collect(@Name(value = "section") String section, @Name(value = "config", defaultValue = "") java.util.Map<String, Object> config) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Start data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'"), Procedure(name : "db.stats.collect", mode : Neo4Net.Procedure.Mode.READ)]
		 public virtual Stream<ActionResult> Collect( string section, IDictionary<string, object> config )
		 {
			  CollectorStateMachine.Result result = CollectorStateMachine( section ).collect( config );
			  return Stream.of( new ActionResult( section, result.Success, result.Message ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Stop data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'") @Procedure(name = "db.stats.stop", mode = org.Neo4Net.procedure.Mode.READ) public java.util.stream.Stream<ActionResult> stop(@Name(value = "section") String section) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Stop data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'"), Procedure(name : "db.stats.stop", mode : Neo4Net.Procedure.Mode.READ)]
		 public virtual Stream<ActionResult> Stop( string section )
		 {
			  CollectorStateMachine.Result result = CollectorStateMachine( section ).stop( long.MaxValue );
			  return Stream.of( new ActionResult( section, result.Success, result.Message ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Clear collected data of a given data section. Valid sections are '" + Sections.QUERIES + "'") @Procedure(name = "db.stats.clear", mode = org.Neo4Net.procedure.Mode.READ) public java.util.stream.Stream<ActionResult> clear(@Name(value = "section") String section) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Clear collected data of a given data section. Valid sections are '" + Sections.QUERIES + "'"), Procedure(name : "db.stats.clear", mode : Neo4Net.Procedure.Mode.READ)]
		 public virtual Stream<ActionResult> Clear( string section )
		 {
			  CollectorStateMachine.Result result = CollectorStateMachine( section ).clear();
			  return Stream.of( new ActionResult( section, result.Success, result.Message ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private QueryCollector collectorStateMachine(String section) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 private QueryCollector CollectorStateMachine( string section )
		 {
			  switch ( section )
			  {
			  case Sections.TOKENS:
			  case Sections.GRAPH_COUNTS:
					throw new InvalidArgumentsException( "Section '%s' does not have to be explicitly collected, it can always be directly retrieved." );
			  case Sections.QUERIES:
					return DataCollector.queryCollector;
			  default:
					throw Sections.UnknownSectionException( section );
			  }
		 }
	}

}