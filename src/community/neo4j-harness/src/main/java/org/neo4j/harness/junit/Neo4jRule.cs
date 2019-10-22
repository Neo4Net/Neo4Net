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
namespace Neo4Net.Harness.junit
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using Suppliers = Neo4Net.Functions.Suppliers;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using Neo4Net.GraphDb.config;

	/// <summary>
	/// A convenience wrapper around <seealso cref="org.Neo4Net.harness.TestServerBuilder"/>, exposing it as a JUnit
	/// <seealso cref="org.junit.Rule rule"/>.
	/// 
	/// Note that it will try to start the web server on the standard 7474 port, but if that is not available
	/// (typically because you already have an instance of Neo4Net running) it will try other ports. Therefore it is necessary
	/// for the test code to use <seealso cref="httpURI()"/> and then <seealso cref="java.net.URI.resolve(string)"/> to create the URIs to be invoked.
	/// </summary>
	public class Neo4NetRule : TestRule, TestServerBuilder
	{
		 private TestServerBuilder _builder;
		 private ServerControls _controls;
		 private System.Func<PrintStream> _dumpLogsOnFailureTarget;

		 internal Neo4NetRule( TestServerBuilder builder )
		 {
			  this._builder = builder;
		 }

		 public Neo4NetRule() : this(TestServerBuilders.newInProcessBuilder())
		 {
		 }

		 public Neo4NetRule( File workingDirectory ) : this( TestServerBuilders.newInProcessBuilder( workingDirectory ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly Neo4NetRule _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( Neo4NetRule outerInstance, Statement @base )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  using ( ServerControls sc = _outerInstance.controls = _outerInstance.builder.newServer() )
				  {
						try
						{
							 @base.evaluate();
						}
						catch ( Exception t )
						{
							 if ( _outerInstance.dumpLogsOnFailureTarget != null )
							 {
								  sc.PrintLogs( _outerInstance.dumpLogsOnFailureTarget.get() );
							 }

							 throw t;
						}
				  }
			 }
		 }

		 public override ServerControls NewServer()
		 {
			  throw new System.NotSupportedException( "The server cannot be manually started via this class, it must be used as a JUnit rule." );
		 }

		 public override Neo4NetRule WithConfig<T1>( Setting<T1> key, string value )
		 {
			  _builder = _builder.withConfig( key, value );
			  return this;
		 }

		 public override Neo4NetRule WithConfig( string key, string value )
		 {
			  _builder = _builder.withConfig( key, value );
			  return this;
		 }

		 public override Neo4NetRule WithExtension( string mountPath, Type extension )
		 {
			  _builder = _builder.withExtension( mountPath, extension );
			  return this;
		 }

		 public override Neo4NetRule WithExtension( string mountPath, string packageName )
		 {
			  _builder = _builder.withExtension( mountPath, packageName );
			  return this;
		 }

		 public override Neo4NetRule WithFixture( File cypherFileOrDirectory )
		 {
			  _builder = _builder.withFixture( cypherFileOrDirectory );
			  return this;
		 }

		 public override Neo4NetRule WithFixture( string fixtureStatement )
		 {
			  _builder = _builder.withFixture( fixtureStatement );
			  return this;
		 }

		 public override Neo4NetRule WithFixture( System.Func<GraphDatabaseService, Void> fixtureFunction )
		 {
			  _builder = _builder.withFixture( fixtureFunction );
			  return this;
		 }

		 public override Neo4NetRule CopyFrom( File sourceDirectory )
		 {
			  _builder = _builder.copyFrom( sourceDirectory );
			  return this;
		 }

		 public override Neo4NetRule WithProcedure( Type procedureClass )
		 {
			  _builder = _builder.withProcedure( procedureClass );
			  return this;
		 }

		 public override Neo4NetRule WithFunction( Type functionClass )
		 {
			  _builder = _builder.withFunction( functionClass );
			  return this;
		 }

		 public override Neo4NetRule WithAggregationFunction( Type functionClass )
		 {
			  _builder = _builder.withAggregationFunction( functionClass );
			  return this;
		 }

		 public virtual Neo4NetRule DumpLogsOnFailure( PrintStream @out )
		 {
			  _dumpLogsOnFailureTarget = () => @out;
			  return this;
		 }

		 public virtual Neo4NetRule DumpLogsOnFailure( System.Func<PrintStream> @out )
		 {
			  _dumpLogsOnFailureTarget = @out;
			  return this;
		 }

		 public virtual URI BoltURI()
		 {
			  if ( _controls == null )
			  {
					throw new System.InvalidOperationException( "Cannot access instance URI before or after the test runs." );
			  }
			  return _controls.boltURI();
		 }

		 public virtual URI HttpURI()
		 {
			  if ( _controls == null )
			  {
					throw new System.InvalidOperationException( "Cannot access instance URI before or after the test runs." );
			  }
			  return _controls.httpURI();
		 }

		 public virtual URI HttpsURI()
		 {
			  if ( _controls == null )
			  {
					throw new System.InvalidOperationException( "Cannot access instance URI before or after the test runs." );
			  }
			  return _controls.httpsURI().orElseThrow(() => new System.InvalidOperationException("HTTPS connector is not configured"));
		 }

		 public virtual IGraphDatabaseService IGraphDatabaseService
		 {
			 get
			 {
				  return _controls.graph();
			 }
		 }

		 public virtual Configuration Config
		 {
			 get
			 {
				  return _controls.config();
			 }
		 }
	}

}