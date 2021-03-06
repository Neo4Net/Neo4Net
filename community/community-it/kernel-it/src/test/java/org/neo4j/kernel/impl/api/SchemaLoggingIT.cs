﻿using System;

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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Org.Neo4j.Function;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexPopulationJob = Org.Neo4j.Kernel.Impl.Api.index.IndexPopulationJob;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using LogMatcherBuilder = Org.Neo4j.Logging.AssertableLogProvider.LogMatcherBuilder;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class SchemaLoggingIT
	{
		private bool InstanceFieldsInitialized = false;

		public SchemaLoggingIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			DbRule = new ImpermanentDatabaseRule( _logProvider );
		}

		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule(logProvider);
		 public ImpermanentDatabaseRule DbRule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUserReadableLabelAndPropertyNames() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogUserReadableLabelAndPropertyNames()
		 {
			  //noinspection deprecation
			  GraphDatabaseAPI db = DbRule.GraphDatabaseAPI;

			  string labelName = "User";
			  string property = "name";

			  // when
			  CreateIndex( db, labelName, property );

			  // then
			  AssertableLogProvider.LogMatcherBuilder match = inLog( typeof( IndexPopulationJob ) );
			  IndexProviderMap indexProviderMap = Db.DependencyResolver.resolveDependency( typeof( IndexProviderMap ) );
			  IndexProvider defaultProvider = indexProviderMap.DefaultProvider;
			  IndexProviderDescriptor providerDescriptor = defaultProvider.ProviderDescriptor;
			  _logProvider.assertAtLeastOnce( match.info( "Index population started: [%s]", ":User(name) [provider: {key=" + providerDescriptor.Key + ", version=" + providerDescriptor.Version + "}]" ) );

			  assertEventually( ( ThrowingSupplier<object, Exception> )() => null, new LogMessageMatcher(this, match, providerDescriptor), 1, TimeUnit.MINUTES );
		 }

		 private static void CreateIndex( GraphDatabaseAPI db, string labelName, string property )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(label(labelName)).on(property).create();
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private class LogMessageMatcher : BaseMatcher<object>
		 {
			 private readonly SchemaLoggingIT _outerInstance;

			  internal const string CREATION_FINISHED = "Index creation finished. Index [%s] is %s.";
			  internal readonly AssertableLogProvider.LogMatcherBuilder Match;
			  internal readonly IndexProviderDescriptor Descriptor;

			  internal LogMessageMatcher( SchemaLoggingIT outerInstance, AssertableLogProvider.LogMatcherBuilder match, IndexProviderDescriptor descriptor )
			  {
				  this._outerInstance = outerInstance;
					this.Match = match;
					this.Descriptor = descriptor;
			  }

			  public override bool Matches( object item )
			  {
					return outerInstance.logProvider.ContainsMatchingLogCall( Match.info( CREATION_FINISHED, ":User(name) [provider: {key=" + Descriptor.Key + ", version=" + Descriptor.Version + "}]", "ONLINE" ) );
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( " expected log message: '" ).appendText( CREATION_FINISHED ).appendText( "', but not found. Messages was: '" ).appendText( outerInstance.logProvider.Serialize() ).appendText(".");
			  }
		 }
	}

}