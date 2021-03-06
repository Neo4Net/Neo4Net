﻿using System.Collections.Generic;

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
namespace Org.Neo4j.@internal.Collector
{

	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Admin = Org.Neo4j.Procedure.Admin;
	using Context = Org.Neo4j.Procedure.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using Mode = Org.Neo4j.Procedure.Mode;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class DataCollectorProcedures
	public class DataCollectorProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public DataCollector dataCollector;
		 public DataCollector DataCollector;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.KernelTransaction transaction;
		 public KernelTransaction Transaction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Retrieve statistical data about the current database. Valid sections are '" + Sections.GRAPH_COUNTS + "', '" + Sections.TOKENS + "', '" + Sections.QUERIES + "', '" + Sections.META + "'") @Procedure(name = "db.stats.retrieve", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<RetrieveResult> retrieve(@Name(value = "section") String section, @Name(value = "config", defaultValue = "") java.util.Map<String, Object> config) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Retrieve statistical data about the current database. Valid sections are '" + Sections.GRAPH_COUNTS + "', '" + Sections.TOKENS + "', '" + Sections.QUERIES + "', '" + Sections.META + "'"), Procedure(name : "db.stats.retrieve", mode : Org.Neo4j.Procedure.Mode.READ)]
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
//ORIGINAL LINE: @Admin @Description("Retrieve all available statistical data about the current database, in an anonymized form.") @Procedure(name = "db.stats.retrieveAllAnonymized", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<RetrieveResult> retrieveAllAnonymized(@Name(value = "graphToken") String graphToken, @Name(value = "config", defaultValue = "") java.util.Map<String, Object> config) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Retrieve all available statistical data about the current database, in an anonymized form."), Procedure(name : "db.stats.retrieveAllAnonymized", mode : Org.Neo4j.Procedure.Mode.READ)]
		 public virtual Stream<RetrieveResult> RetrieveAllAnonymized( string graphToken, IDictionary<string, object> config )
		 {
			  if ( string.ReferenceEquals( graphToken, null ) || graphToken.Equals( "" ) )
			  {
					throw new InvalidArgumentsException( "Graph token must be a non-empty string" );
			  }

			  return Stream.of( MetaSection.Retrieve( graphToken, DataCollector.kernel, DataCollector.queryCollector.numSilentQueryDrops() ), GraphCountsSection.Retrieve(DataCollector.kernel, Anonymizer.IDS), QueriesSection.retrieve(DataCollector.queryCollector.Data, new IdAnonymizer(Transaction.tokenRead()), RetrieveConfig.Of(config).MaxInvocations) ).flatMap(x => x);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Retrieve the status of all available collector daemons, for this database.") @Procedure(name = "db.stats.status", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<StatusResult> status()
		 [Description("Retrieve the status of all available collector daemons, for this database."), Procedure(name : "db.stats.status", mode : Org.Neo4j.Procedure.Mode.READ)]
		 public virtual Stream<StatusResult> Status()
		 {
			  CollectorStateMachine.Status status = DataCollector.queryCollector.status();
			  return Stream.of( new StatusResult( Sections.QUERIES, status.Message, Collections.emptyMap() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Start data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'") @Procedure(name = "db.stats.collect", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<ActionResult> collect(@Name(value = "section") String section, @Name(value = "config", defaultValue = "") java.util.Map<String, Object> config) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Start data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'"), Procedure(name : "db.stats.collect", mode : Org.Neo4j.Procedure.Mode.READ)]
		 public virtual Stream<ActionResult> Collect( string section, IDictionary<string, object> config )
		 {
			  CollectorStateMachine.Result result = CollectorStateMachine( section ).collect( config );
			  return Stream.of( new ActionResult( section, result.Success, result.Message ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Stop data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'") @Procedure(name = "db.stats.stop", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<ActionResult> stop(@Name(value = "section") String section) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Stop data collection of a given data section. Valid sections are '" + Sections.QUERIES + "'"), Procedure(name : "db.stats.stop", mode : Org.Neo4j.Procedure.Mode.READ)]
		 public virtual Stream<ActionResult> Stop( string section )
		 {
			  CollectorStateMachine.Result result = CollectorStateMachine( section ).stop( long.MaxValue );
			  return Stream.of( new ActionResult( section, result.Success, result.Message ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Clear collected data of a given data section. Valid sections are '" + Sections.QUERIES + "'") @Procedure(name = "db.stats.clear", mode = org.neo4j.procedure.Mode.READ) public java.util.stream.Stream<ActionResult> clear(@Name(value = "section") String section) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Clear collected data of a given data section. Valid sections are '" + Sections.QUERIES + "'"), Procedure(name : "db.stats.clear", mode : Org.Neo4j.Procedure.Mode.READ)]
		 public virtual Stream<ActionResult> Clear( string section )
		 {
			  CollectorStateMachine.Result result = CollectorStateMachine( section ).clear();
			  return Stream.of( new ActionResult( section, result.Success, result.Message ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private QueryCollector collectorStateMachine(String section) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
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