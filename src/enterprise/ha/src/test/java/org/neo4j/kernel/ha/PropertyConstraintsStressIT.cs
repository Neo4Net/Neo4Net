using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;


	using ComException = Neo4Net.com.ComException;
	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using TransientTransactionFailureException = Neo4Net.GraphDb.TransientTransactionFailureException;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using IGraphDatabaseServiceCleaner = Neo4Net.Test.GraphDatabaseServiceCleaner;
	using Neo4Net.Test.OtherThreadExecutor;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.ArrayUtil.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.loop;

	/// <summary>
	/// This test stress tests unique constraints in a setup where writes are being issued against a slave.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class PropertyConstraintsStressIT
	public class PropertyConstraintsStressIT
	{
		 private static readonly System.Func<int, string> STRING_VALUE_GENERATOR = new FuncAnonymousInnerClass();

		 private class FuncAnonymousInnerClass : System.Func<int, string>
		 {
			 public override string apply( int value )
			 {
				  return "value" + value;
			 }

			 public override string ToString()
			 {
				  return "STRING";
			 }
		 }
		 private static readonly System.Func<int, Number> NUMBER_VALUE_GENERATOR = new FuncAnonymousInnerClass2();

		 private class FuncAnonymousInnerClass2 : System.Func<int, Number>
		 {
			 public override Number apply( int value )
			 {
				  return value;
			 }

			 public override string ToString()
			 {
				  return "NUMBER";
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public ConstraintOperations constraintOps;
		 public ConstraintOperations ConstraintOps;
		 [Parameter(1)]
		 public System.Func<int, object> ValueGenerator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule();
		 public static readonly ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.concurrent.OtherThreadRule<Object> slaveWork = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>();
		 public OtherThreadRule<object> SlaveWork = new OtherThreadRule<object>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.concurrent.OtherThreadRule<Object> masterWork = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>();
		 public OtherThreadRule<object> MasterWork = new OtherThreadRule<object>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.RepeatRule repeater = new org.Neo4Net.test.rule.RepeatRule();
		 public RepeatRule Repeater = new RepeatRule();

		 protected internal ClusterManager.ManagedCluster Cluster;

		 private readonly int _repetitions = 1;
		 /// <summary>
		 /// Configure how long to run the test. </summary>
		 private static long _runtime = Long.getLong( "Neo4Net.PropertyConstraintsStressIT.runtime", TimeUnit.SECONDS.toMillis( 10 ) );

		 /// <summary>
		 /// Label or relationship type to constrain for the current iteration of the test. </summary>
		 private volatile string _labelOrRelType = "Foo";

		 /// <summary>
		 /// Property key to constrain for the current iteration of the test. </summary>
		 private volatile string _property;

		 private readonly AtomicInteger _roundNo = new AtomicInteger( 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}:{1}") public static Iterable<Object[]> params()
		 public static IEnumerable<object[]> Params()
		 {
			  IList<object[]> data = new List<object[]>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (System.Func<int, ?> values : array(STRING_VALUE_GENERATOR, NUMBER_VALUE_GENERATOR))
			  foreach ( System.Func<int, ?> values in array( STRING_VALUE_GENERATOR, NUMBER_VALUE_GENERATOR ) )
			  {
					foreach ( ConstraintOperations operations in array( UNIQUE_PROPERTY_CONSTRAINT_OPS, NODE_PROPERTY_EXISTENCE_CONSTRAINT_OPS, REL_PROPERTY_EXISTENCE_CONSTRAINT_OPS ) )
					{
						 data.Add( array( operations, values ) );
					}
			  }
			  return data;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Cluster = ClusterRule.withSharedSetting( HaSettings.PullInterval, "0" ).startCluster();
			  ClearData();
		 }

		 private void ClearData()
		 {
			  HighlyAvailableGraphDatabase db = Cluster.Master;
			  IGraphDatabaseServiceCleaner.cleanDatabaseContent( db );
			  Cluster.sync();
		 }

		 /* The different orders and delays in the below variations try to stress all known scenarios, as well as
		  * of course stress for unknown concurrency issues. See the exception handling structure further below
		  * for explanations. */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatRule.Repeat(times = REPETITIONS) @Test public void shouldNotAllowConstraintsViolationsUnderStress_A() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowConstraintsViolationsUnderStressA()
		 {
			  ShouldNotAllowConstraintsViolationsUnderStress( new OperationAnonymousInnerClass( this ) );
		 }

		 private class OperationAnonymousInnerClass : Operation
		 {
			 private readonly PropertyConstraintsStressIT _outerInstance;

			 public OperationAnonymousInnerClass( PropertyConstraintsStressIT outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override void perform()
			 {
				  constraintCreation = _outerInstance.masterWork.execute( _outerInstance.createConstraint( master ) );
				  constraintViolation = _outerInstance.slaveWork.execute( _outerInstance.performConstraintViolatingInserts( slave ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatRule.Repeat(times = REPETITIONS) @Test public void shouldNotAllowConstraintsViolationsUnderStress_B() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowConstraintsViolationsUnderStressB()
		 {
			  ShouldNotAllowConstraintsViolationsUnderStress( new OperationAnonymousInnerClass2( this ) );
		 }

		 private class OperationAnonymousInnerClass2 : Operation
		 {
			 private readonly PropertyConstraintsStressIT _outerInstance;

			 public OperationAnonymousInnerClass2( PropertyConstraintsStressIT outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override void perform()
			 {
				  constraintViolation = _outerInstance.slaveWork.execute( _outerInstance.performConstraintViolatingInserts( slave ) );
				  constraintCreation = _outerInstance.masterWork.execute( _outerInstance.createConstraint( master ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatRule.Repeat(times = REPETITIONS) @Test public void shouldNotAllowConstraintsViolationsUnderStress_C() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowConstraintsViolationsUnderStressC()
		 {
			  ShouldNotAllowConstraintsViolationsUnderStress( new OperationAnonymousInnerClass3( this ) );
		 }

		 private class OperationAnonymousInnerClass3 : Operation
		 {
			 private readonly PropertyConstraintsStressIT _outerInstance;

			 public OperationAnonymousInnerClass3( PropertyConstraintsStressIT outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override void perform()
			 {
				  constraintCreation = _outerInstance.masterWork.execute( _outerInstance.createConstraint( master ) );

				  try
				  {
						Thread.Sleep( ThreadLocalRandom.current().Next(100) );
				  }
				  catch ( InterruptedException )
				  {
				  }

				  constraintViolation = _outerInstance.slaveWork.execute( _outerInstance.performConstraintViolatingInserts( slave ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatRule.Repeat(times = REPETITIONS) @Test public void shouldNotAllowConstraintsViolationsUnderStress_D() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowConstraintsViolationsUnderStressD()
		 {
			  ShouldNotAllowConstraintsViolationsUnderStress( new OperationAnonymousInnerClass4( this ) );
		 }

		 private class OperationAnonymousInnerClass4 : Operation
		 {
			 private readonly PropertyConstraintsStressIT _outerInstance;

			 public OperationAnonymousInnerClass4( PropertyConstraintsStressIT outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override void perform()
			 {
				  constraintViolation = _outerInstance.slaveWork.execute( _outerInstance.performConstraintViolatingInserts( slave ) );

				  try
				  {
						Thread.Sleep( ThreadLocalRandom.current().Next(100) );
				  }
				  catch ( InterruptedException )
				  {
				  }

				  constraintCreation = _outerInstance.masterWork.execute( _outerInstance.createConstraint( master ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatRule.Repeat(times = REPETITIONS) @Test public void shouldNotAllowConstraintsViolationsUnderStress_E() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowConstraintsViolationsUnderStressE()
		 {
			  ShouldNotAllowConstraintsViolationsUnderStress( new OperationAnonymousInnerClass5( this ) );
		 }

		 private class OperationAnonymousInnerClass5 : Operation
		 {
			 private readonly PropertyConstraintsStressIT _outerInstance;

			 public OperationAnonymousInnerClass5( PropertyConstraintsStressIT outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override void perform()
			 {
				  constraintCreation = _outerInstance.masterWork.execute( _outerInstance.createConstraint( master ) );

				  try
				  {
						Thread.Sleep( 2000 );
				  }
				  catch ( InterruptedException )
				  {
				  }

				  constraintViolation = _outerInstance.slaveWork.execute( _outerInstance.performConstraintViolatingInserts( slave ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatRule.Repeat(times = REPETITIONS) @Test public void shouldNotAllowConstraintsViolationsUnderStress_F() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowConstraintsViolationsUnderStressF()
		 {
			  ShouldNotAllowConstraintsViolationsUnderStress( new OperationAnonymousInnerClass6( this ) );
		 }

		 private class OperationAnonymousInnerClass6 : Operation
		 {
			 private readonly PropertyConstraintsStressIT _outerInstance;

			 public OperationAnonymousInnerClass6( PropertyConstraintsStressIT outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override void perform()
			 {
				  constraintViolation = _outerInstance.slaveWork.execute( _outerInstance.performConstraintViolatingInserts( slave ) );

				  try
				  {
						Thread.Sleep( 2000 );
				  }
				  catch ( InterruptedException )
				  {
				  }

				  constraintCreation = _outerInstance.masterWork.execute( _outerInstance.createConstraint( master ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldNotAllowConstraintsViolationsUnderStress(Operation ops) throws Exception
		 private void ShouldNotAllowConstraintsViolationsUnderStress( Operation ops )
		 {
			  // Given
			  HighlyAvailableGraphDatabase master = Cluster.Master;
			  HighlyAvailableGraphDatabase slave = Cluster.AnySlave;

			  // Because this is a brute-force test, we run for a user-specified time, and consider ourselves successful if
			  // no failure is found.
			  long end = DateTimeHelper.CurrentUnixTimeMillis() + _runtime;
			  int successfulAttempts = 0;
			  while ( end > DateTimeHelper.CurrentUnixTimeMillis() )
			  {
					// Each round of this loop:
					//  - on a slave, creates a set of nodes with a label/property combo with all-unique values
					//  - on the master, creates a property constraint
					//  - on a slave, while the master is creating the constraint, starts transactions that will
					//    violate the constraint

					{
					//setup :
						 // Set a target property for the constraint for this round
						 SetLabelAndPropertyForNextRound();

						 // Ensure slave has constraints from previous round
						 Cluster.sync();

						 // Create the initial data that *does not* violate the constraint
						 try
						 {
							  SlaveWork.execute( PerformConstraintCompliantInserts( slave ) ).get();
						 }
						 catch ( ExecutionException e )
						 {
							  if ( Exceptions.contains( e, "could not connect", typeof( ComException ) ) )
							  { // We're in a weird cluster state. The slave should be able to succeed if trying again
									continue;
							  }
						 }
					}

					{
					//stress :
						 ops.Perform();

						 // And then ensure all is well
						 AssertConstraintsNotViolated( ops.ConstraintCreation, ops.ConstraintViolation, master );
						 successfulAttempts++;
					}
			  }

			  assertThat( successfulAttempts, greaterThan( 0 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertConstraintsNotViolated(java.util.concurrent.Future<bool> constraintCreation, java.util.concurrent.Future<int> constraintViolation, HighlyAvailableGraphDatabase master) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void AssertConstraintsNotViolated( Future<bool> constraintCreation, Future<int> constraintViolation, HighlyAvailableGraphDatabase master )
		 {
			  bool constraintCreationFailed = constraintCreation.get();

			  int txSuccessCount = constraintViolation.get();

			  if ( constraintCreationFailed )
			  {
					// Constraint creation failed, some of the violating operations should have been successful.
					assertThat( txSuccessCount, greaterThan( 0 ) );
					// And the graph should contain some duplicates.
					assertThat( ConstraintOps.isValid( master, _labelOrRelType, _property ), equalTo( false ) );
			  }
			  else
			  {
					// Constraint creation was successful, all the violating operations should have failed.
					assertThat( txSuccessCount, equalTo( 0 ) );
					// And the graph should not contain duplicates.
					assertThat( ConstraintOps.isValid( master, _labelOrRelType, _property ), equalTo( true ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.test.OtherThreadExecutor.WorkerCommand<Object,bool> createConstraint(final HighlyAvailableGraphDatabase master)
		 private WorkerCommand<object, bool> CreateConstraint( HighlyAvailableGraphDatabase master )
		 {
			  return ConstraintOps.createConstraint( master, _labelOrRelType, _property );
		 }

		 private WorkerCommand<object, int> PerformConstraintCompliantInserts( HighlyAvailableGraphDatabase slave )
		 {
			  return PerformInserts( slave, true );
		 }

		 private WorkerCommand<object, int> PerformConstraintViolatingInserts( HighlyAvailableGraphDatabase slave )
		 {
			  return PerformInserts( slave, false );
		 }

		 /// <summary>
		 /// Inserts a bunch of new nodes with the label and property key currently set in the fields in this class, where
		 /// running this method twice will insert nodes with duplicate property values, assuming property key or label has
		 /// not changed.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.test.OtherThreadExecutor.WorkerCommand<Object,int> performInserts(final HighlyAvailableGraphDatabase slave, final boolean constraintCompliant)
		 private WorkerCommand<object, int> PerformInserts( HighlyAvailableGraphDatabase slave, bool constraintCompliant )
		 {
			  return state =>
			  {
				int i = 0;

				try
				{
					 for ( ; i < 100; i++ )
					 {
						  using ( Transaction tx = slave.BeginTx() )
						  {
								ConstraintOps.createEntity( slave, _labelOrRelType, _property, ValueGenerator.apply( i ), constraintCompliant );
								tx.success();
						  }
					 }
				}
				catch ( Exception e ) when ( e is TransactionFailureException || e is TransientTransactionFailureException || e is ComException || e is ConstraintViolationException )
				{
					 // TransactionFailureException and TransientTransactionFailureException
					 //   Swallowed on purpose, we except it to fail sometimes due to either
					 //    - constraint violation on master
					 //    - concurrent schema operation on master

					 // ConstraintViolationException
					 //   Constraint violation detected on slave while building transaction

					 // ComException
					 //   Happens sometimes, cause:
					 //   - The lock session requested to start is already in use.
					 //     Please retry your request in a few seconds.
				}
				return i;
			  };
		 }

		 private void SetLabelAndPropertyForNextRound()
		 {
			  _property = "Key-" + _roundNo.incrementAndGet();
			  _labelOrRelType = "Foo-" + _roundNo.get();
		 }

		 private abstract class Operation
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 Master = outerInstance.Cluster.Master;
				 Slave = outerInstance.Cluster.AnySlave;
			 }

			 private readonly PropertyConstraintsStressIT _outerInstance;

			 public Operation( PropertyConstraintsStressIT outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			  internal HighlyAvailableGraphDatabase Master;
			  internal HighlyAvailableGraphDatabase Slave;

			  internal Future<bool> ConstraintCreation;
			  internal Future<int> ConstraintViolation;

			  internal abstract void Perform();
		 }

		 internal interface ConstraintOperations
		 {
			  void CreateEntity( HighlyAvailableGraphDatabase db, string type, string propertyKey, object value, bool constraintCompliant );

			  bool IsValid( HighlyAvailableGraphDatabase master, string type, string property );

			  WorkerCommand<object, bool> CreateConstraint( HighlyAvailableGraphDatabase db, string type, string property );
		 }

		 private static readonly ConstraintOperations UNIQUE_PROPERTY_CONSTRAINT_OPS = new ConstraintOperationsAnonymousInnerClass();

		 private class ConstraintOperationsAnonymousInnerClass : ConstraintOperations
		 {
			 public void createEntity( HighlyAvailableGraphDatabase db, string type, string propertyKey, object value, bool constraintCompliant )
			 {
				  Db.createNode( label( type ) ).setProperty( propertyKey, value );
			 }

			 public bool isValid( HighlyAvailableGraphDatabase db, string type, string property )
			 {
				  using ( Transaction tx = Db.beginTx() )
				  {
						ISet<object> values = new HashSet<object>();
						using ( ResourceIterator<Node> nodes = Db.findNodes( label( type ) ) )
						{
							 foreach ( Node node in loop( nodes ) )
							 {
								  object value = node.GetProperty( property );
								  if ( values.Contains( value ) )
								  {
										return false;
								  }
								  values.Add( value );
							 }
						}

						tx.Success();
				  }
				  return true;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.test.OtherThreadExecutor.WorkerCommand<Object,bool> createConstraint(final HighlyAvailableGraphDatabase db, final String type, final String property)
			 public WorkerCommand<object, bool> createConstraint( HighlyAvailableGraphDatabase db, string type, string property )
			 {
				  return state =>
				  {
					bool constraintCreationFailed = false;

					try
					{
						using ( Transaction tx = Db.beginTx() )
						{
							 Db.schema().constraintFor(label(type)).assertPropertyIsUnique(property).create();
							 tx.success();
						}
					}
					catch ( ConstraintViolationException )
					{
					/* Unable to create constraint since it is not consistent with existing data. */
						 constraintCreationFailed = true;
					}

					return constraintCreationFailed;
				  };
			 }

			 public override string ToString()
			 {
				  return "UNIQUE_PROPERTY_CONSTRAINT";
			 }
		 }

		 private static readonly ConstraintOperations NODE_PROPERTY_EXISTENCE_CONSTRAINT_OPS = new ConstraintOperationsAnonymousInnerClass2();

		 private class ConstraintOperationsAnonymousInnerClass2 : ConstraintOperations
		 {
			 public void createEntity( HighlyAvailableGraphDatabase db, string type, string propertyKey, object value, bool constraintCompliant )
			 {
				  Node node = Db.createNode( label( type ) );
				  if ( constraintCompliant )
				  {
						node.SetProperty( propertyKey, value );
				  }
			 }

			 public bool isValid( HighlyAvailableGraphDatabase db, string type, string property )
			 {
				  using ( Transaction tx = Db.beginTx() )
				  {
						foreach ( Node node in loop( Db.findNodes( label( type ) ) ) )
						{
							 if ( !node.HasProperty( property ) )
							 {
								  return false;
							 }
						}

						tx.Success();
				  }
				  return true;
			 }

			 public WorkerCommand<object, bool> createConstraint( HighlyAvailableGraphDatabase db, string type, string property )
			 {
				  string query = format( "CREATE CONSTRAINT ON (n:`%s`) ASSERT exists(n.`%s`)", type, property );
				  return CreatePropertyExistenceConstraintCommand( db, query );
			 }

			 public override string ToString()
			 {
				  return "NODE_PROPERTY_EXISTENCE_CONSTRAINT";
			 }
		 }

		 private static readonly ConstraintOperations REL_PROPERTY_EXISTENCE_CONSTRAINT_OPS = new ConstraintOperationsAnonymousInnerClass3();

		 private class ConstraintOperationsAnonymousInnerClass3 : ConstraintOperations
		 {
			 public void createEntity( HighlyAvailableGraphDatabase db, string type, string propertyKey, object value, bool constraintCompliant )
			 {
				  Node start = Db.createNode();
				  Node end = Db.createNode();
				  Relationship relationship = start.CreateRelationshipTo( end, withName( type ) );
				  if ( constraintCompliant )
				  {
						relationship.SetProperty( propertyKey, value );
				  }
			 }

			 public bool isValid( HighlyAvailableGraphDatabase db, string type, string property )
			 {
				  using ( Transaction tx = Db.beginTx() )
				  {
						foreach ( Relationship relationship in Db.AllRelationships )
						{
							 if ( relationship.Type.name().Equals(type) )
							 {
								  if ( !relationship.HasProperty( property ) )
								  {
										return false;
								  }
							 }
						}
						tx.Success();
				  }
				  return true;
			 }

			 public WorkerCommand<object, bool> createConstraint( HighlyAvailableGraphDatabase db, string type, string property )
			 {
				  string query = format( "CREATE CONSTRAINT ON ()-[r:`%s`]-() ASSERT exists(r.`%s`)", type, property );
				  return CreatePropertyExistenceConstraintCommand( db, query );
			 }

			 public override string ToString()
			 {
				  return "RELATIONSHIP_PROPERTY_EXISTENCE_CONSTRAINT";
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.test.OtherThreadExecutor.WorkerCommand<Object,bool> createPropertyExistenceConstraintCommand(final org.Neo4Net.graphdb.GraphDatabaseService db, final String query)
		 private static WorkerCommand<object, bool> CreatePropertyExistenceConstraintCommand( IGraphDatabaseService db, string query )
		 {
			  return state =>
			  {
				bool constraintCreationFailed = false;

				try
				{
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.execute( query );
						 tx.success();
					}
				}
				catch ( QueryExecutionException e )
				{
					 if ( Exceptions.rootCause( e ) is ConstraintValidationException )
					 {
						  // Unable to create constraint since it is not consistent with existing data
						  constraintCreationFailed = true;
					 }
					 else
					 {
						  throw e;
					 }
				}

				return constraintCreationFailed;
			  };
		 }
	}

}