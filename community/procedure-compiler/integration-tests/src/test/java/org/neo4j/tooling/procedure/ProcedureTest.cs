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
namespace Org.Neo4j.Tooling.procedure
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Config = Org.Neo4j.driver.v1.Config;
	using Driver = Org.Neo4j.driver.v1.Driver;
	using GraphDatabase = Org.Neo4j.driver.v1.GraphDatabase;
	using Session = Org.Neo4j.driver.v1.Session;
	using StatementResult = Org.Neo4j.driver.v1.StatementResult;
	using Neo4jRule = Org.Neo4j.Harness.junit.Neo4jRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using Procedures = Org.Neo4j.Tooling.procedure.procedures.valid.Procedures;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;


	public class ProcedureTest
	{
		 private static readonly Type _proceduresClass = typeof( Procedures );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.harness.junit.Neo4jRule graphDb = new org.neo4j.harness.junit.Neo4jRule().dumpLogsOnFailure(() -> System.out).withProcedure(PROCEDURES_CLASS);
		 public Neo4jRule GraphDb = new Neo4jRule().dumpLogsOnFailure(() => System.out).withProcedure(_proceduresClass);
		 private string _procedureNamespace = _proceduresClass.Assembly.GetName().Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void calls_simplistic_procedure()
		 public virtual void CallsSimplisticProcedure()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {

					StatementResult result = session.run( "CALL " + _procedureNamespace + ".theAnswer()" );

					assertThat( result.single().get("value").asLong() ).isEqualTo(42L);
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void calls_procedures_with_simple_input_type_returning_void()
		 public virtual void CallsProceduresWithSimpleInputTypeReturningVoid()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {

					session.run( "CALL " + _procedureNamespace + ".simpleInput00()" );
					session.run( "CALL " + _procedureNamespace + ".simpleInput01('string')" );
					session.run( "CALL " + _procedureNamespace + ".simpleInput02(42)" );
					session.run( "CALL " + _procedureNamespace + ".simpleInput03(42)" );
					session.run( "CALL " + _procedureNamespace + ".simpleInput04(4.2)" );
					session.run( "CALL " + _procedureNamespace + ".simpleInput05(true)" );
					session.run( "CALL " + _procedureNamespace + ".simpleInput06(false)" );
					session.run( "CALL " + _procedureNamespace + ".simpleInput07({foo:'bar'})" );
					session.run( "MATCH (n)            CALL " + _procedureNamespace + ".simpleInput08(n) RETURN n" );
					session.run( "MATCH p=(()-[r]->()) CALL " + _procedureNamespace + ".simpleInput09(p) RETURN p" );
					session.run( "MATCH ()-[r]->()     CALL " + _procedureNamespace + ".simpleInput10(r) RETURN r" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void calls_procedures_with_different_modes_returning_void()
		 public virtual void CallsProceduresWithDifferentModesReturningVoid()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					session.run( "CALL " + _procedureNamespace + ".performsWrites()" );
					session.run( "CALL " + _procedureNamespace + ".defaultMode()" );
					session.run( "CALL " + _procedureNamespace + ".readMode()" );
					session.run( "CALL " + _procedureNamespace + ".writeMode()" );
					session.run( "CALL " + _procedureNamespace + ".schemaMode()" );
					session.run( "CALL " + _procedureNamespace + ".dbmsMode()" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void calls_procedures_with_simple_input_type_returning_record_with_primitive_fields()
		 public virtual void CallsProceduresWithSimpleInputTypeReturningRecordWithPrimitiveFields()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {

					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput11('string') YIELD field04 AS p RETURN p" ).single() ).NotNull;
					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput12(42)" ).single() ).NotNull;
					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput13(42)" ).single() ).NotNull;
					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput14(4.2)" ).single() ).NotNull;
					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput15(true)" ).single() ).NotNull;
					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput16(false)" ).single() ).NotNull;
					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput17({foo:'bar'})" ).single() ).NotNull;
					assertThat( session.run( "CALL " + _procedureNamespace + ".simpleInput21()" ).single() ).NotNull;
			  }

		 }

		 private Config Configuration()
		 {
			  return Config.build().withEncryptionLevel(Config.EncryptionLevel.NONE).withConnectionTimeout(10, TimeUnit.SECONDS).toConfig();
		 }

	}

}