using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.enterprise.builtinprocs
{

	using UncaughtCheckedException = Neo4Net.Functions.UncaughtCheckedException;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using AuthorizationViolationException = Neo4Net.GraphDb.security.AuthorizationViolationException;
	using Neo4Net.Helpers.Collections;
	using ProcedureSignature = Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature;
	using UserFunctionSignature = Neo4Net.Internal.Kernel.Api.procs.UserFunctionSignature;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Statement = Neo4Net.Kernel.api.Statement;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Neo4Net.Kernel.api.net.TrackedNetworkConnection;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using QuerySnapshot = Neo4Net.Kernel.api.query.QuerySnapshot;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Admin = Neo4Net.Procedure.Admin;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.ThrowingFunction.catchThrown;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.ThrowingFunction.throwIfPresent;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.enterprise.builtinprocs.QueryId.fromExternalString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.enterprise.builtinprocs.QueryId.ofInternalId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.procedure.Mode.DBMS;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class EnterpriseBuiltInDbmsProcedures
	public class EnterpriseBuiltInDbmsProcedures
	{
		 private const int HARD_CHAR_LIMIT = 2048;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.graphdb.DependencyResolver resolver;
		 public DependencyResolver Resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.kernel.internal.GraphDatabaseAPI graph;
		 public GraphDatabaseAPI Graph;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.internal.kernel.api.security.SecurityContext securityContext;
		 public SecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Attaches a map of data to the transaction. The data will be printed when listing queries, and " + "inserted into the query log.") @Procedure(name = "dbms.setTXMetaData", mode = DBMS) public void setTXMetaData(@Name(value = "data") java.util.Map<String,Object> data)
		 [Description("Attaches a map of data to the transaction. The data will be printed when listing queries, and " + "inserted into the query log."), Procedure(name : "dbms.setTXMetaData", mode : DBMS)]
		 public virtual void setTXMetaData( IDictionary<string, object> data )
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  int totalCharSize = data.SetOfKeyValuePairs().Select(e => e.Key.length() + e.Value.ToString().Length).Sum();

			  if ( totalCharSize >= HARD_CHAR_LIMIT )
			  {
					throw new System.ArgumentException( format( "Invalid transaction meta-data, expected the total number of chars for " + "keys and values to be less than %d, got %d", HARD_CHAR_LIMIT, totalCharSize ) );
			  }

			  CurrentTx.MetaData = data;
		 }

		 [Description("Provides attached transaction metadata."), Procedure(name : "dbms.getTXMetaData", mode : DBMS)]
		 public virtual Stream<MetadataResult> GetTXMetaData()
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  using ( Statement statement = CurrentTx.acquireStatement() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return Stream.of( statement.QueryRegistration().MetaData ).map(MetadataResult::new);
			  }
		 }

		 private KernelTransaction CurrentTx
		 {
			 get
			 {
				  return Graph.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
			 }
		 }

		 [Description("List all accepted network connections at this instance that are visible to the user."), Procedure(name : "dbms.listConnections", mode : DBMS)]
		 public virtual Stream<ListConnectionResult> ListConnections()
		 {
			  SecurityContext.assertCredentialsNotExpired();

			  NetworkConnectionTracker connectionTracker = ConnectionTracker;
			  ZoneId timeZone = ConfiguredTimeZone;

			  return connectionTracker.ActiveConnections().Where(connection => IsAdminOrSelf(connection.username())).Select(connection => new ListConnectionResult(connection, timeZone));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Kill network connection with the given connection id.") @Procedure(name = "dbms.killConnection", mode = DBMS) public java.util.stream.Stream<ConnectionTerminationResult> killConnection(@Name("id") String id)
		 [Description("Kill network connection with the given connection id."), Procedure(name : "dbms.killConnection", mode : DBMS)]
		 public virtual Stream<ConnectionTerminationResult> KillConnection( string id )
		 {
			  return KillConnections( singletonList( id ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Kill all network connections with the given connection ids.") @Procedure(name = "dbms.killConnections", mode = DBMS) public java.util.stream.Stream<ConnectionTerminationResult> killConnections(@Name("ids") java.util.List<String> ids)
		 [Description("Kill all network connections with the given connection ids."), Procedure(name : "dbms.killConnections", mode : DBMS)]
		 public virtual Stream<ConnectionTerminationResult> KillConnections( IList<string> ids )
		 {
			  SecurityContext.assertCredentialsNotExpired();

			  NetworkConnectionTracker connectionTracker = ConnectionTracker;

			  return ids.Select( id => KillConnection( id, connectionTracker ) );
		 }

		 private NetworkConnectionTracker ConnectionTracker
		 {
			 get
			 {
				  return Graph.DependencyResolver.resolveDependency( typeof( NetworkConnectionTracker ) );
			 }
		 }

		 private ConnectionTerminationResult KillConnection( string id, NetworkConnectionTracker connectionTracker )
		 {
			  TrackedNetworkConnection connection = connectionTracker.Get( id );
			  if ( connection != null )
			  {
					if ( IsAdminOrSelf( connection.Username() ) )
					{
						 connection.Close();
						 return new ConnectionTerminationResult( id, connection.Username() );
					}
					throw new AuthorizationViolationException( PERMISSION_DENIED );
			  }
			  return new ConnectionTerminationFailedResult( id );
		 }

		 [Description("List all user functions in the DBMS."), Procedure(name : "dbms.functions", mode : DBMS)]
		 public virtual Stream<FunctionResult> ListFunctions()
		 {
			  SecurityContext.assertCredentialsNotExpired();
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Graph.DependencyResolver.resolveDependency( typeof( Procedures ) ).AllFunctions.OrderBy( System.Collections.IComparer.comparing( a => a.name().ToString() ) ).Select(FunctionResult::new);
		 }

		 public class FunctionResult
		 {
			  public readonly string Name;
			  public readonly string Signature;
			  public readonly string Description;
			  public readonly IList<string> Roles;

			  internal FunctionResult( UserFunctionSignature signature )
			  {
					this.Name = signature.Name().ToString();
					this.Signature = signature.ToString();
					this.Description = signature.Description().orElse("");
					Roles = Stream.of( "admin", "reader", "editor", "publisher", "architect" ).collect( toList() );
					( ( IList<string> )Roles ).AddRange( Arrays.asList( signature.Allowed() ) );
			  }
		 }

		 [Description("List all procedures in the DBMS."), Procedure(name : "dbms.procedures", mode : DBMS)]
		 public virtual Stream<ProcedureResult> ListProcedures()
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  Procedures procedures = Graph.DependencyResolver.resolveDependency( typeof( Procedures ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return procedures.AllProcedures.OrderBy( System.Collections.IComparer.comparing( a => a.name().ToString() ) ).Select(ProcedureResult::new);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public static class ProcedureResult
		 public class ProcedureResult
		 {
			  // These two procedures are admin procedures but may be executed for your own user,
			  // this is not documented anywhere but we cannot change the behaviour in a point release
			  internal static readonly IList<string> AdminProcedures = Arrays.asList( "changeUserPassword", "listRolesForUser" );

			  public readonly string Name;
			  public readonly string Signature;
			  public readonly string Description;
			  public readonly IList<string> Roles;
			  public readonly string Mode;

			  public ProcedureResult( ProcedureSignature signature )
			  {
					this.Name = signature.Name().ToString();
					this.Signature = signature.ToString();
					this.Description = signature.Description().orElse("");
					this.Mode = signature.Mode().ToString();
					Roles = new List<string>();
					switch ( signature.Mode() )
					{
					case DBMS:
						 if ( signature.Admin() || IsAdminProcedure(signature.Name().name()) )
						 {
							  Roles.Add( "admin" );
						 }
						 else
						 {
							  Roles.Add( "reader" );
							  Roles.Add( "editor" );
							  Roles.Add( "publisher" );
							  Roles.Add( "architect" );
							  Roles.Add( "admin" );
							  ( ( IList<string> )Roles ).AddRange( Arrays.asList( signature.Allowed() ) );
						 }
						 break;
					case DEFAULT:
					case READ:
						 Roles.Add( "reader" );
						goto case WRITE;
					case WRITE:
						 Roles.Add( "editor" );
						 Roles.Add( "publisher" );
						goto case SCHEMA;
					case SCHEMA:
						 Roles.Add( "architect" );
						goto default;
					default:
						 Roles.Add( "admin" );
						 ( ( IList<string> )Roles ).AddRange( Arrays.asList( signature.Allowed() ) );
					 break;
					}
			  }

			  internal virtual bool IsAdminProcedure( string procedureName )
			  {
					return Name.StartsWith( "dbms.security.", StringComparison.Ordinal ) && AdminProcedures.Contains( procedureName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Updates a given setting value. Passing an empty value will result in removing the configured value " + "and falling back to the default value. Changes will not persist and will be lost if the server is restarted.") @Procedure(name = "dbms.setConfigValue", mode = DBMS) public void setConfigValue(@Name("setting") String setting, @Name("value") String value)
		 [Description("Updates a given setting value. Passing an empty value will result in removing the configured value " + "and falling back to the default value. Changes will not persist and will be lost if the server is restarted."), Procedure(name : "dbms.setConfigValue", mode : DBMS)]
		 public virtual void SetConfigValue( string setting, string value )
		 {
			  Config config = Resolver.resolveDependency( typeof( Config ) );
			  config.UpdateDynamicSetting( setting, value, "dbms.setConfigValue" ); // throws if something goes wrong
		 }

		 /*
		 ==================================================================================
		  */

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<QueryStatusResult> listQueries() throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Description("List all queries currently executing at this instance that are visible to the user."), Procedure(name : "dbms.listQueries", mode : DBMS)]
		 public virtual Stream<QueryStatusResult> ListQueries()
		 {
			  SecurityContext.assertCredentialsNotExpired();

			  EmbeddedProxySPI nodeManager = Resolver.resolveDependency( typeof( EmbeddedProxySPI ) );
			  ZoneId zoneId = ConfiguredTimeZone;
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return KernelTransactions.activeTransactions().stream().flatMap(KernelTransactionHandle::executingQueries).filter(query => IsAdminOrSelf(query.username())).map(catchThrown(typeof(InvalidArgumentsException), query => new QueryStatusResult(query, nodeManager, zoneId)));
			  }
			  catch ( UncaughtCheckedException uncaught )
			  {
					throwIfPresent( uncaught.GetCauseIfOfType( typeof( InvalidArgumentsException ) ) );
					throw uncaught;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<TransactionStatusResult> listTransactions() throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Description("List all transactions currently executing at this instance that are visible to the user."), Procedure(name : "dbms.listTransactions", mode : DBMS)]
		 public virtual Stream<TransactionStatusResult> ListTransactions()
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<KernelTransactionHandle> handles = KernelTransactions.activeTransactions().Where(transaction => IsAdminOrSelf(transaction.subject().username())).collect(toSet());

					IDictionary<KernelTransactionHandle, IList<QuerySnapshot>> handleQuerySnapshotsMap = handles.ToDictionary( identity(), TransactionQueries );

					TransactionDependenciesResolver transactionBlockerResolvers = new TransactionDependenciesResolver( handleQuerySnapshotsMap );

					ZoneId zoneId = ConfiguredTimeZone;

					return handles.Select( catchThrown( typeof( InvalidArgumentsException ), tx => new TransactionStatusResult( tx, transactionBlockerResolvers, handleQuerySnapshotsMap, zoneId ) ) );
			  }
			  catch ( UncaughtCheckedException uncaught )
			  {
					throwIfPresent( uncaught.GetCauseIfOfType( typeof( InvalidArgumentsException ) ) );
					throw uncaught;
			  }
		 }

		 private static System.Func<KernelTransactionHandle, IList<QuerySnapshot>> TransactionQueries
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  return transactionHandle => transactionHandle.executingQueries().map(ExecutingQuery::snapshot).collect(toList());
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("List the active lock requests granted for the transaction executing the query with the given query id.") @Procedure(name = "dbms.listActiveLocks", mode = DBMS) public java.util.stream.Stream<ActiveLocksResult> listActiveLocks(@Name("queryId") String queryId) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("List the active lock requests granted for the transaction executing the query with the given query id."), Procedure(name : "dbms.listActiveLocks", mode : DBMS)]
		 public virtual Stream<ActiveLocksResult> ListActiveLocks( string queryId )
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  try
			  {
					long id = fromExternalString( queryId ).kernelQueryId();
					return GetActiveTransactions( tx => ExecutingQueriesWithId( id, tx ) ).flatMap( this.getActiveLocksForQuery );
			  }
			  catch ( UncaughtCheckedException uncaught )
			  {
					throwIfPresent( uncaught.GetCauseIfOfType( typeof( InvalidArgumentsException ) ) );
					throw uncaught;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Kill all transactions executing the query with the given query id.") @Procedure(name = "dbms.killQuery", mode = DBMS) public java.util.stream.Stream<QueryTerminationResult> killQuery(@Name("id") String idText) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Kill all transactions executing the query with the given query id."), Procedure(name : "dbms.killQuery", mode : DBMS)]
		 public virtual Stream<QueryTerminationResult> KillQuery( string idText )
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  try
			  {
					long queryId = fromExternalString( idText ).kernelQueryId();

					ISet<Pair<KernelTransactionHandle, ExecutingQuery>> querys = GetActiveTransactions( tx => ExecutingQueriesWithId( queryId, tx ) ).collect( toSet() );
					bool killQueryVerbose = Resolver.resolveDependency( typeof( Config ) ).get( GraphDatabaseSettings.kill_query_verbose );
					if ( killQueryVerbose && querys.Count == 0 )
					{
						 return Stream.builder<QueryTerminationResult>().add(new QueryFailedTerminationResult(fromExternalString(idText))).build();
					}
					return querys.Select( catchThrown( typeof( InvalidArgumentsException ), this.killQueryTransaction ) );
			  }
			  catch ( UncaughtCheckedException uncaught )
			  {
					throwIfPresent( uncaught.GetCauseIfOfType( typeof( InvalidArgumentsException ) ) );
					throw uncaught;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Kill all transactions executing a query with any of the given query ids.") @Procedure(name = "dbms.killQueries", mode = DBMS) public java.util.stream.Stream<QueryTerminationResult> killQueries(@Name("ids") java.util.List<String> idTexts) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Kill all transactions executing a query with any of the given query ids."), Procedure(name : "dbms.killQueries", mode : DBMS)]
		 public virtual Stream<QueryTerminationResult> KillQueries( IList<string> idTexts )
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  try
			  {

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<long> queryIds = idTexts.Select( catchThrown( typeof( InvalidArgumentsException ), QueryId.fromExternalString ) ).Select( catchThrown( typeof( InvalidArgumentsException ), QueryId::kernelQueryId ) ).collect( toSet() );

					ISet<QueryTerminationResult> terminatedQuerys = GetActiveTransactions( tx => ExecutingQueriesWithIds( queryIds, tx ) ).map( catchThrown( typeof( InvalidArgumentsException ), this.killQueryTransaction ) ).collect( toSet() );
					bool killQueryVerbose = Resolver.resolveDependency( typeof( Config ) ).get( GraphDatabaseSettings.kill_query_verbose );
					if ( killQueryVerbose && terminatedQuerys.Count != idTexts.Count )
					{
						 foreach ( string id in idTexts )
						 {
							  if ( terminatedQuerys.noneMatch( query => query.queryId.Equals( id ) ) )
							  {
									terminatedQuerys.Add( new QueryFailedTerminationResult( fromExternalString( id ) ) );
							  }
						 }
					}
					return terminatedQuerys.stream();
			  }
			  catch ( UncaughtCheckedException uncaught )
			  {
					throwIfPresent( uncaught.GetCauseIfOfType( typeof( InvalidArgumentsException ) ) );
					throw uncaught;
			  }
		 }

		 private Stream<Pair<KernelTransactionHandle, T>> GetActiveTransactions<T>( System.Func<KernelTransactionHandle, Stream<T>> selector )
		 {
			  return GetActiveTransactions( Graph.DependencyResolver ).stream().flatMap(tx => selector(tx).map(data => Pair.of(tx, data)));
		 }

		 private static Stream<ExecutingQuery> ExecutingQueriesWithIds( ISet<long> ids, KernelTransactionHandle txHandle )
		 {
			  return txHandle.ExecutingQueries().filter(q => ids.Contains(q.internalQueryId()));
		 }

		 private static Stream<ExecutingQuery> ExecutingQueriesWithId( long id, KernelTransactionHandle txHandle )
		 {
			  return txHandle.ExecutingQueries().filter(q => q.internalQueryId() == id);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private QueryTerminationResult killQueryTransaction(org.Neo4Net.helpers.collection.Pair<org.Neo4Net.kernel.api.KernelTransactionHandle, org.Neo4Net.kernel.api.query.ExecutingQuery> pair) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 private QueryTerminationResult KillQueryTransaction( Pair<KernelTransactionHandle, ExecutingQuery> pair )
		 {
			  ExecutingQuery query = pair.Other();
			  if ( IsAdminOrSelf( query.Username() ) )
			  {
					pair.First().markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated);
					return new QueryTerminationResult( ofInternalId( query.InternalQueryId() ), query.Username() );
			  }
			  else
			  {
					throw new AuthorizationViolationException( PERMISSION_DENIED );
			  }
		 }

		 private Stream<ActiveLocksResult> GetActiveLocksForQuery( Pair<KernelTransactionHandle, ExecutingQuery> pair )
		 {
			  ExecutingQuery query = pair.Other();
			  if ( IsAdminOrSelf( query.Username() ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return pair.First().activeLocks().map(ActiveLocksResult::new);
			  }
			  else
			  {
					throw new AuthorizationViolationException( PERMISSION_DENIED );
			  }
		 }

		 private KernelTransactions KernelTransactions
		 {
			 get
			 {
				  return Resolver.resolveDependency( typeof( KernelTransactions ) );
			 }
		 }

		 // ----------------- helpers ---------------------

		 public static Stream<TransactionTerminationResult> TerminateTransactionsForValidUser( DependencyResolver dependencyResolver, string username, KernelTransaction currentTx )
		 {
			  long terminatedCount = GetActiveTransactions( dependencyResolver ).Where( tx => tx.subject().hasUsername(username) && !tx.isUnderlyingTransaction(currentTx) ).Select(tx => tx.markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated)).Where(marked => marked).Count();
			  return Stream.of( new TransactionTerminationResult( username, terminatedCount ) );
		 }

		 public static ISet<KernelTransactionHandle> GetActiveTransactions( DependencyResolver dependencyResolver )
		 {
			  return dependencyResolver.ResolveDependency( typeof( KernelTransactions ) ).activeTransactions();
		 }

		 public static Stream<TransactionResult> CountTransactionByUsername( Stream<string> usernames )
		 {
			  return usernames.collect( Collectors.groupingBy( identity(), Collectors.counting() ) ).entrySet().Select(entry => new TransactionResult(entry.Key, entry.Value)
			 );
		 }

		 private ZoneId ConfiguredTimeZone
		 {
			 get
			 {
				  Config config = Resolver.resolveDependency( typeof( Config ) );
				  return config.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
			 }
		 }

		 private bool IsAdminOrSelf( string username )
		 {
			  return SecurityContext.Admin || SecurityContext.subject().hasUsername(username);
		 }

		 private void AssertAdminOrSelf( string username )
		 {
			  if ( !IsAdminOrSelf( username ) )
			  {
					throw new AuthorizationViolationException( PERMISSION_DENIED );
			  }
		 }

		 public class QueryTerminationResult
		 {
			  public readonly string QueryId;
			  public readonly string Username;
			  public string Message = "Query found";

			  public QueryTerminationResult( QueryId queryId, string username )
			  {
					this.QueryId = queryId.ToString();
					this.Username = username;
			  }
		 }

		 public class QueryFailedTerminationResult : QueryTerminationResult
		 {
			  public QueryFailedTerminationResult( QueryId queryId ) : base( queryId, "n/a" )
			  {
					base.Message = "No Query found with this id";
			  }
		 }

		 public class TransactionResult
		 {
			  public readonly string Username;
			  public readonly long? ActiveTransactions;

			  internal TransactionResult( string username, long? activeTransactions )
			  {
					this.Username = username;
					this.ActiveTransactions = activeTransactions;
			  }
		 }

		 public class TransactionTerminationResult
		 {
			  public readonly string Username;
			  public readonly long? TransactionsTerminated;

			  internal TransactionTerminationResult( string username, long? transactionsTerminated )
			  {
					this.Username = username;
					this.TransactionsTerminated = transactionsTerminated;
			  }
		 }

		 public class MetadataResult
		 {
			  public readonly IDictionary<string, object> Metadata;

			  internal MetadataResult( IDictionary<string, object> metadata )
			  {
					this.Metadata = metadata;
			  }
		 }
	}

}