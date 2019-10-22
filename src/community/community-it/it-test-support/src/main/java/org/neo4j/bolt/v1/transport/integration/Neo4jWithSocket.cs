using System;
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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using ExternalResource = org.junit.rules.ExternalResource;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.BoltConnector.EncryptionLevel.OPTIONAL;

	public class Neo4NetWithSocket : ExternalResource
	{
		 public const string DEFAULT_CONNECTOR_KEY = "bolt";

		 private System.Func<FileSystemAbstraction> _fileSystemProvider;
		 private readonly System.Action<IDictionary<string, string>> _configure;
		 private readonly TestDirectory _testDirectory;
		 private TestGraphDatabaseFactory _graphDatabaseFactory;
		 private IGraphDatabaseService _gdb;
		 private File _workingDirectory;
		 private ConnectorPortRegister _connectorRegister;

		 public Neo4NetWithSocket( Type testClass ) : this(testClass, settings ->
		 {
		  {
		  });
		 }

		 public Neo4NetWithSocket( Type testClass, System.Action<IDictionary<string, string>> configure ) : this( testClass, new TestGraphDatabaseFactory(), configure )
		 {
		 }

		 public Neo4NetWithSocket( Type testClass, TestGraphDatabaseFactory graphDatabaseFactory, System.Action<IDictionary<string, string>> configure ) : this( testClass, graphDatabaseFactory, EphemeralFileSystemAbstraction::new, configure )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 public Neo4NetWithSocket( Type testClass, TestGraphDatabaseFactory graphDatabaseFactory, System.Func<FileSystemAbstraction> fileSystemProvider, System.Action<IDictionary<string, string>> configure )
		 {
			  this._testDirectory = TestDirectory.testDirectory( testClass, fileSystemProvider() );
			  this._graphDatabaseFactory = graphDatabaseFactory;
			  this._fileSystemProvider = fileSystemProvider;
			  this._configure = configure;
			  this._workingDirectory = DefaultWorkingDirectory();
		 }

		 public virtual FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return this._graphDatabaseFactory.FileSystem;
			 }
		 }

		 public virtual File WorkingDirectory
		 {
			 get
			 {
				  return _workingDirectory;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement statement, final org.junit.runner.Description description)
		 public override Statement Apply( Statement statement, Description description )
		 {
			  Statement testMethod = new StatementAnonymousInnerClass( this, statement, description );

			  Statement testMethodWithBeforeAndAfter = base.Apply( testMethod, description );

			  return _testDirectory.apply( testMethodWithBeforeAndAfter, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly Neo4NetWithSocket _outerInstance;

			 private Statement _statement;
			 private Description _description;

			 public StatementAnonymousInnerClass( Neo4NetWithSocket outerInstance, Statement statement, Description description )
			 {
				 this.outerInstance = outerInstance;
				 this._statement = statement;
				 this._description = description;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  // If this is used as class rule then getMethodName() returns null, so use
				  // getClassName() instead.
				  string name = _description.MethodName != null ? _description.MethodName : _description.ClassName;
				  _outerInstance.workingDirectory = _outerInstance.testDirectory.directory( name );
				  outerInstance.EnsureDatabase(settings =>
				  {
				  });
				  try
				  {
						_statement.evaluate();
				  }
				  finally
				  {
						outerInstance.ShutdownDatabase();
				  }
			 }
		 }

		 public virtual HostnamePort LookupConnector( string connectorKey )
		 {
			  return _connectorRegister.getLocalAddress( connectorKey );
		 }

		 public virtual HostnamePort LookupDefaultConnector()
		 {
			  return _connectorRegister.getLocalAddress( DEFAULT_CONNECTOR_KEY );
		 }

		 public virtual void ShutdownDatabase()
		 {
			  try
			  {
					if ( _gdb != null )
					{
						 _gdb.shutdown();
					}
			  }
			  finally
			  {
					_connectorRegister = null;
					_gdb = null;
			  }
		 }

		 public virtual void EnsureDatabase( System.Action<IDictionary<string, string>> overrideSettingsFunction )
		 {
			  if ( _gdb != null )
			  {
					return;
			  }

			  IDictionary<string, string> settings = Configure( overrideSettingsFunction );
			  File storeDir = new File( _workingDirectory, "storeDir" );
			  _graphDatabaseFactory.FileSystem = _fileSystemProvider.get();
			  _gdb = _graphDatabaseFactory.newImpermanentDatabaseBuilder( storeDir ).setConfig( settings ).newGraphDatabase();
			  _connectorRegister = ( ( GraphDatabaseAPI ) _gdb ).DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
		 }

		 private IDictionary<string, string> Configure( System.Action<IDictionary<string, string>> overrideSettingsFunction )
		 {
			  IDictionary<string, string> settings = new Dictionary<string, string>();
			  settings[( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).type.name()] = "BOLT";
			  settings[( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).enabled.name()] = "true";
			  settings[( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).listen_address.name()] = "localhost:0";
			  settings[( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).encryption_level.name()] = OPTIONAL.name();
			  _configure.accept( settings );
			  overrideSettingsFunction( settings );
			  return settings;
		 }

		 public virtual IGraphDatabaseService IGraphDatabaseService()
		 {
			  return _gdb;
		 }

		 private File DefaultWorkingDirectory()
		 {
			  try
			  {
					return _testDirectory.prepareDirectoryForTest( "default" );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}