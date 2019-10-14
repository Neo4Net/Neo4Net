using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.tools.applytx
{

	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using Result = Neo4Net.Consistency.ConsistencyCheckService.Result;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using Args = Neo4Net.Helpers.Args;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using Neo4Net.Kernel.impl.util;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using ArgsCommand = Neo4Net.tools.console.input.ArgsCommand;
	using ConsoleInput = Neo4Net.tools.console.input.ConsoleInput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleAdapter.onShutdown;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tools.console.input.ConsoleUtil.NO_PROMPT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tools.console.input.ConsoleUtil.oneCommand;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tools.console.input.ConsoleUtil.staticPrompt;

	/// <summary>
	/// Tool for rebuilding database from transaction logs onto a new store. Transaction can be applied interactively,
	/// i.e. applied up to any transaction id and consistency checked at any point. Also there are other utilities,
	/// such as printing record structure at any point as well.
	/// 
	/// Running this tool will eventually go into a mode where it's awaiting user input, so typed commands
	/// will control what happens. Type help to get more information.
	/// </summary>
	public class DatabaseRebuildTool
	{
		 private readonly Stream @in;
		 private readonly PrintStream @out;
		 private readonly PrintStream _err;

		 public DatabaseRebuildTool() : this(System.in, System.out, System.err)
		 {
		 }

		 public DatabaseRebuildTool( Stream @in, PrintStream @out, PrintStream err )
		 {
			  this.@in = @in;
			  this.@out = @out;
			  this._err = err;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] arguments) throws Exception
		 public static void Main( string[] arguments )
		 {
			  ( new DatabaseRebuildTool() ).Run(arguments);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String... arguments) throws Exception
		 public virtual void Run( params string[] arguments )
		 {
			  if ( arguments.Length == 0 )
			  {
					Console.Error.WriteLine( "Tool for rebuilding database from transaction logs onto a new store" );
					Console.Error.WriteLine( "Example: dbrebuild --from path/to/some.db --to path/to/new.db apply next" );
					Console.Error.WriteLine( "         dbrebuild --from path/to/some.db --to path/to/new.db -i" );
					Console.Error.WriteLine( "          --from : which db to use as source for reading transactions" );
					Console.Error.WriteLine( "            --to : where to build the new db" );
					Console.Error.WriteLine( "  --overwrite-to : always starts from empty 'to' db" );
					Console.Error.WriteLine( "              -i : interactive mode (enter a shell)" );
					return;
			  }

			  Args args = Args.withFlags( "i", "overwrite-to" ).parse( arguments );
			  File fromPath = GetFrom( args );
			  File toPath = GetTo( args );
			  GraphDatabaseBuilder dbBuilder = NewDbBuilder( toPath, args );
			  bool interactive = args.GetBoolean( "i" );
			  if ( interactive && args.Orphans().Count > 0 )
			  {
					throw new System.ArgumentException( "No additional commands allowed in interactive mode" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("resource") java.io.InputStream input = interactive ? in : oneCommand(args.orphansAsArray());
			  Stream input = interactive ? @in : oneCommand( args.OrphansAsArray() );
			  LifeSupport life = new LifeSupport();
			  ConsoleInput consoleInput = Console( fromPath, dbBuilder, input, interactive ? staticPrompt( "# " ) : NO_PROMPT, life );
			  life.Start();
			  try
			  {
					consoleInput.WaitFor();
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File getTo(org.neo4j.helpers.Args args) throws java.io.IOException
		 private File GetTo( Args args )
		 {
			  string to = args.Get( "to" );
			  if ( string.ReferenceEquals( to, null ) )
			  {
					to = "target/db-from-apply-txs";
					_err.println( "Defaulting --to to " + to );
			  }
			  File toPath = new File( to );
			  if ( args.GetBoolean( "overwrite-to" ) )
			  {
					FileUtils.deleteRecursively( toPath );
			  }
			  return toPath;
		 }

		 private static File GetFrom( Args args )
		 {
			  string from = args.Get( "from" );
			  if ( string.ReferenceEquals( from, null ) )
			  {
					throw new System.ArgumentException( "Missing --from i.e. from where to read transaction logs" );
			  }
			  return new File( from );
		 }

		 private static GraphDatabaseBuilder NewDbBuilder( File path, Args args )
		 {
			  GraphDatabaseBuilder builder = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(path);
			  foreach ( KeyValuePair<string, string> entry in args.AsMap().SetOfKeyValuePairs() )
			  {
					if ( entry.Key.StartsWith( "D" ) )
					{
						 string key = entry.Key.substring( 1 );
						 string value = entry.Value;
						 builder = builder.SetConfig( key, value );
					}
			  }
			  return builder;
		 }

		 private class Store
		 {
			  internal readonly GraphDatabaseAPI Db;
			  internal readonly StoreAccess Access;
			  internal readonly DatabaseLayout DatabaseLayout;

			  internal Store( GraphDatabaseBuilder dbBuilder )
			  {
					this.Db = ( GraphDatabaseAPI ) dbBuilder.NewGraphDatabase();
					this.Access = ( new StoreAccess( Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores() ) ).initialize();
					this.DatabaseLayout = Db.databaseLayout();
			  }

			  public virtual void Shutdown()
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.tools.console.input.ConsoleInput console(final java.io.File fromPath, final org.neo4j.graphdb.factory.GraphDatabaseBuilder dbBuilder, java.io.InputStream in, org.neo4j.kernel.impl.util.Listener<java.io.PrintStream> prompt, org.neo4j.kernel.lifecycle.LifeSupport life)
		 private ConsoleInput Console( File fromPath, GraphDatabaseBuilder dbBuilder, Stream @in, Listener<PrintStream> prompt, LifeSupport life )
		 {
			  // We must have this indirection here since in order to perform CC (one of the commands) we must shut down
			  // the database and let CC instantiate its own to run on. After that completes the db
			  // should be restored. The commands has references to providers of things to accommodate for this.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<Store> store = new java.util.concurrent.atomic.AtomicReference<>(new Store(dbBuilder));
			  AtomicReference<Store> store = new AtomicReference<Store>( new Store( dbBuilder ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Func<org.neo4j.kernel.impl.store.StoreAccess> storeAccess = () -> store.get().access;
			  System.Func<StoreAccess> storeAccess = () => store.get().access;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Func<org.neo4j.kernel.internal.GraphDatabaseAPI> dbAccess = () -> store.get().db;
			  System.Func<GraphDatabaseAPI> dbAccess = () => store.get().db;

			  ConsoleInput consoleInput = life.Add( new ConsoleInput( @in, @out, prompt ) );
			  consoleInput.Add( "apply", new ApplyTransactionsCommand( fromPath, dbAccess ) );
			  consoleInput.Add( DumpRecordsCommand.NAME, new DumpRecordsCommand( storeAccess ) );
			  consoleInput.Add( "cc", new ArgsCommandAnonymousInnerClass( this, dbBuilder, store ) );
			  life.Add( onShutdown( () => store.get().shutdown() ) );
			  return consoleInput;
		 }

		 private class ArgsCommandAnonymousInnerClass : ArgsCommand
		 {
			 private readonly DatabaseRebuildTool _outerInstance;

			 private GraphDatabaseBuilder _dbBuilder;
			 private AtomicReference<Store> _store;

			 public ArgsCommandAnonymousInnerClass( DatabaseRebuildTool outerInstance, GraphDatabaseBuilder dbBuilder, AtomicReference<Store> store )
			 {
				 this.outerInstance = outerInstance;
				 this._dbBuilder = dbBuilder;
				 this._store = store;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(org.neo4j.helpers.Args action, java.io.PrintStream out) throws Exception
			 public override void run( Args action, PrintStream @out )
			 {
				  DatabaseLayout databaseLayout = _store.get().databaseLayout;
				  _store.get().shutdown();
				  try
				  {
						ConsistencyCheckService.Result result = ( new ConsistencyCheckService() ).runFullConsistencyCheck(databaseLayout, Config.defaults(), ProgressMonitorFactory.textual(@out), FormattedLogProvider.toOutputStream(System.out), false);
						@out.println( result.Successful ? "consistent" : "INCONSISTENT" );
				  }
				  finally
				  {
						_store.set( new Store( _dbBuilder ) );
				  }
			 }

			 public override string ToString()
			 {
				  return "Runs consistency check on the database for data that has been applied up to this point";
			 }
		 }
	}

}