using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Exceptions
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Classification.ClientError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Classification.ClientNotification;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Classification.DatabaseError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Classification.TransientError;

	/// <summary>
	/// This is the codification of all available surface-api status codes. If you are throwing an error to a user through
	/// one of the key APIs, you should opt for using or adding an error code here.
	/// 
	/// Each <seealso cref="Status"/> has an associated category, represented by the inner enums in this class.
	/// Each <seealso cref="Status"/> also has an associated <seealso cref="Classification"/> which defines meta-data about the code, such
	/// as if the error was caused by a user or the database (and later on if the code denotes an error or merely a warning).
	/// 
	/// This class is not part of the public Neo4j API, and backwards compatibility for using it as a Java class is not
	/// guaranteed. Instead, the automatically generated documentation derived from this class and available in the Neo4j
	/// manual should be considered a user-level API.
	/// </summary>
	public interface Status
	{
		 /*
	
		 On naming...
	
		 These are public status codes and users' error handling will rely on the precise names. Therefore, please take
		 care over categorisation and classification and make sure not to introduce duplicates.
	
		 Broadly, the naming convention here uses one of three types of name:
	
		 1. For unexpected events, name with a leading noun followed by a short problem term in the past tense. For example,
		    EntityNotFound or LockSessionExpired. As a variant, names may omit the leading noun; in this case, the current
		    ongoing operation is implied.
	
		 2. For conditions that prevent the current ongoing operation from being performed (or being performed correctly),
		    start with a leading noun (as above) and follow with an adjective. For example, DatabaseUnavailable. The
		    leading noun may again be omitted and additionally a clarifying suffix may be added. For example,
		    ForbiddenOnReadOnlyDatabase.
	
		 3. For more general errors which have a well-understood or generic term available, the form XxxError may be used.
		    For example, SyntaxError or TokenNameError.
	
		 Where possible, evaluate naming decisions based on the order of the items above. Therefore, if it is possible to
		 provide a type (1) name, do so, otherwise fall back to type (2) or type (3)
	
		 Side note about HTTP: where possible, borrow words or terms from HTTP status codes. Be careful to make sure these
		 use a similar meaning however as a major benefit of doing this is to ease communication of concepts to users.
	
		 If you are unsure, please contact the Driver Team.
	
		 */

		 // TODO: rework the names and uses of Invalid and InvalidFormat and reconsider their categorisation (ClientError
		 // TODO: MUST be resolvable by the user, do we need ProtocolError/DriverError?)

		 Status_Code Code();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static Status statusCodeOf(Throwable e)
	//	 {
	//		  do
	//		  {
	//				if (e instanceof Status.HasStatus)
	//				{
	//					 return ((Status.HasStatus) e).status();
	//				}
	//				e = e.getCause();
	//		  }
	//		  while (e != null);
	//
	//		  return null;
	//	 }
	}

	 public sealed class Status_Network : Status
	 {
		  // transient
		  public static readonly Status_Network CommunicationError = new Status_Network( "CommunicationError", InnerEnum.CommunicationError, TransientError, "An unknown network failure occurred, a retry may resolve the issue." );

		  private static readonly IList<Status_Network> valueList = new List<Status_Network>();

		  static Status_Network()
		  {
			  valueList.Add( CommunicationError );
		  }

		  public enum InnerEnum
		  {
			  CommunicationError
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;
		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Network( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Network> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Network valueOf( string name )
		 {
			 foreach ( Status_Network enumInstance in Status_Network.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_Request : Status
	 {
		  // client
		  public static readonly Status_Request Invalid = new Status_Request( "Invalid", InnerEnum.Invalid, ClientError, "The client provided an invalid request." );
		  public static readonly Status_Request InvalidFormat = new Status_Request( "InvalidFormat", InnerEnum.InvalidFormat, ClientError, "The client provided a request that was missing required fields, or had values that are not allowed." );
		  public static readonly Status_Request TransactionRequired = new Status_Request( "TransactionRequired", InnerEnum.TransactionRequired, ClientError, "The request cannot be performed outside of a transaction, and there is no transaction present to " + "use. Wrap your request in a transaction and retry." );
		  public static readonly Status_Request InvalidUsage = new Status_Request( "InvalidUsage", InnerEnum.InvalidUsage, ClientError, "The client made a request but did not consume outgoing buffers in a timely fashion." );
		  public static readonly Status_Request NoThreadsAvailable = new Status_Request( "NoThreadsAvailable", InnerEnum.NoThreadsAvailable, TransientError, "There are no available threads to serve this request at the moment. You can retry at a later time " + "or consider increasing max thread pool size for bolt connector(s)." );

		  private static readonly IList<Status_Request> valueList = new List<Status_Request>();

		  static Status_Request()
		  {
			  valueList.Add( Invalid );
			  valueList.Add( InvalidFormat );
			  valueList.Add( TransactionRequired );
			  valueList.Add( InvalidUsage );
			  valueList.Add( NoThreadsAvailable );
		  }

		  public enum InnerEnum
		  {
			  Invalid,
			  InvalidFormat,
			  TransactionRequired,
			  InvalidUsage,
			  NoThreadsAvailable
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;
		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Request( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Request> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Request valueOf( string name )
		 {
			 foreach ( Status_Request enumInstance in Status_Request.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_Transaction : Status
	 {
		  // client errors
		  public static readonly Status_Transaction TransactionNotFound = new Status_Transaction( "TransactionNotFound", InnerEnum.TransactionNotFound, ClientError, "The request referred to a transaction that does not exist." );
		  public static readonly Status_Transaction TransactionAccessedConcurrently = new Status_Transaction( "TransactionAccessedConcurrently", InnerEnum.TransactionAccessedConcurrently, ClientError, "There were concurrent requests accessing the same transaction, which is not allowed." );
		  public static readonly Status_Transaction ForbiddenDueToTransactionType = new Status_Transaction( "ForbiddenDueToTransactionType", InnerEnum.ForbiddenDueToTransactionType, ClientError, "The transaction is of the wrong type to service the request. For instance, a transaction that has " + "had schema modifications performed in it cannot be used to subsequently perform data operations, " + "and vice versa." );
		  public static readonly Status_Transaction TransactionValidationFailed = new Status_Transaction( "TransactionValidationFailed", InnerEnum.TransactionValidationFailed, ClientError, "Transaction changes did not pass validation checks" );
		  public static readonly Status_Transaction TransactionHookFailed = new Status_Transaction( "TransactionHookFailed", InnerEnum.TransactionHookFailed, ClientError, "Transaction hook failure." );
		  public static readonly Status_Transaction TransactionMarkedAsFailed = new Status_Transaction( "TransactionMarkedAsFailed", InnerEnum.TransactionMarkedAsFailed, ClientError, "Transaction was marked as both successful and failed. Failure takes precedence and so this " + "transaction was rolled back although it may have looked like it was going to be committed" );
		  public static readonly Status_Transaction TransactionTimedOut = new Status_Transaction( "TransactionTimedOut", InnerEnum.TransactionTimedOut, ClientError, "The transaction has not completed within the specified timeout (dbms.transaction.timeout). You may want to retry with a longer " + "timeout." );
		  public static readonly Status_Transaction InvalidBookmark = new Status_Transaction( "InvalidBookmark", InnerEnum.InvalidBookmark, ClientError, "Supplied bookmark cannot be interpreted. You should only supply a bookmark previously that was " + "previously generated by Neo4j. Maybe you have generated your own bookmark, " + "or modified a bookmark since it was generated by Neo4j." );

		  // database errors
		  public static readonly Status_Transaction TransactionStartFailed = new Status_Transaction( "TransactionStartFailed", InnerEnum.TransactionStartFailed, DatabaseError, "The database was unable to start the transaction." );
		  public static readonly Status_Transaction TransactionRollbackFailed = new Status_Transaction( "TransactionRollbackFailed", InnerEnum.TransactionRollbackFailed, DatabaseError, "The database was unable to roll back the transaction." );
		  public static readonly Status_Transaction TransactionCommitFailed = new Status_Transaction( "TransactionCommitFailed", InnerEnum.TransactionCommitFailed, DatabaseError, "The database was unable to commit the transaction." );
		  public static readonly Status_Transaction TransactionLogError = new Status_Transaction( "TransactionLogError", InnerEnum.TransactionLogError, DatabaseError, "The database was unable to write transaction to log." );

		  // transient errors
		  public static readonly Status_Transaction LockSessionExpired = new Status_Transaction( "LockSessionExpired", InnerEnum.LockSessionExpired, TransientError, "The lock session under which this transaction was started is no longer valid." );
		  public static readonly Status_Transaction DeadlockDetected = new Status_Transaction( "DeadlockDetected", InnerEnum.DeadlockDetected, TransientError, "This transaction, and at least one more transaction, has acquired locks in a way that it will wait " + "indefinitely, and the database has aborted it. Retrying this transaction will most likely be " + "successful." );
		  public static readonly Status_Transaction InstanceStateChanged = new Status_Transaction( "InstanceStateChanged", InnerEnum.InstanceStateChanged, TransientError, "Transactions rely on assumptions around the state of the Neo4j instance they " + "execute on. For instance, transactions in a cluster may expect that " + "they are executing on an instance that can perform writes. However, " + "instances may change state while the transaction is running. This causes " + "assumptions the instance has made about how to execute the transaction " + "to be violated - meaning the transaction must be rolled " + "back. If you see this error, you should retry your operation in a new transaction." );
		  public static readonly Status_Transaction ConstraintsChanged = new Status_Transaction( "ConstraintsChanged", InnerEnum.ConstraintsChanged, TransientError, "Database constraints changed since the start of this transaction" );
		  public static readonly Status_Transaction Outdated = new Status_Transaction( "Outdated", InnerEnum.Outdated, TransientError, "Transaction has seen state which has been invalidated by applied updates while " + "transaction was active. Transaction may succeed if retried." );
		  public static readonly Status_Transaction LockClientStopped = new Status_Transaction( "LockClientStopped", InnerEnum.LockClientStopped, TransientError, "The transaction has been terminated, so no more locks can be acquired. This can occur because the " + "transaction ran longer than the configured transaction timeout, or because a human operator manually " + "terminated the transaction, or because the database is shutting down." );
		  public static readonly Status_Transaction LockAcquisitionTimeout = new Status_Transaction( "LockAcquisitionTimeout", InnerEnum.LockAcquisitionTimeout, TransientError, "Unable to acquire lock within configured timeout (dbms.lock.acquisition.timeout)." );
		  public static readonly Status_Transaction Terminated = new Status_Transaction( "Terminated", InnerEnum.Terminated, TransientError, "Explicitly terminated by the user." );
		  public static readonly Status_Transaction Interrupted = new Status_Transaction( "Interrupted", InnerEnum.Interrupted, TransientError, "Interrupted while waiting." );

		  private static readonly IList<Status_Transaction> valueList = new List<Status_Transaction>();

		  static Status_Transaction()
		  {
			  valueList.Add( TransactionNotFound );
			  valueList.Add( TransactionAccessedConcurrently );
			  valueList.Add( ForbiddenDueToTransactionType );
			  valueList.Add( TransactionValidationFailed );
			  valueList.Add( TransactionHookFailed );
			  valueList.Add( TransactionMarkedAsFailed );
			  valueList.Add( TransactionTimedOut );
			  valueList.Add( InvalidBookmark );
			  valueList.Add( TransactionStartFailed );
			  valueList.Add( TransactionRollbackFailed );
			  valueList.Add( TransactionCommitFailed );
			  valueList.Add( TransactionLogError );
			  valueList.Add( LockSessionExpired );
			  valueList.Add( DeadlockDetected );
			  valueList.Add( InstanceStateChanged );
			  valueList.Add( ConstraintsChanged );
			  valueList.Add( Outdated );
			  valueList.Add( LockClientStopped );
			  valueList.Add( LockAcquisitionTimeout );
			  valueList.Add( Terminated );
			  valueList.Add( Interrupted );
		  }

		  public enum InnerEnum
		  {
			  TransactionNotFound,
			  TransactionAccessedConcurrently,
			  ForbiddenDueToTransactionType,
			  TransactionValidationFailed,
			  TransactionHookFailed,
			  TransactionMarkedAsFailed,
			  TransactionTimedOut,
			  InvalidBookmark,
			  TransactionStartFailed,
			  TransactionRollbackFailed,
			  TransactionCommitFailed,
			  TransactionLogError,
			  LockSessionExpired,
			  DeadlockDetected,
			  InstanceStateChanged,
			  ConstraintsChanged,
			  Outdated,
			  LockClientStopped,
			  LockAcquisitionTimeout,
			  Terminated,
			  Interrupted
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Transaction( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Transaction> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Transaction valueOf( string name )
		 {
			 foreach ( Status_Transaction enumInstance in Status_Transaction.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_Statement : Status
	 {
		  // client errors
		  public static readonly Status_Statement SyntaxError = new Status_Statement( "SyntaxError", InnerEnum.SyntaxError, ClientError, "The statement contains invalid or unsupported syntax." );
		  public static readonly Status_Statement SemanticError = new Status_Statement( "SemanticError", InnerEnum.SemanticError, ClientError, "The statement is syntactically valid, but expresses something that the database cannot do." );
		  public static readonly Status_Statement ParameterMissing = new Status_Statement( "ParameterMissing", InnerEnum.ParameterMissing, ClientError, "The statement refers to a parameter that was not provided in the request." );
		  public static readonly Status_Statement ConstraintVerificationFailed = new Status_Statement( "ConstraintVerificationFailed", InnerEnum.ConstraintVerificationFailed, ClientError, "A constraint imposed by the statement is violated by the data in the database." );
		  public static readonly Status_Statement EntityNotFound = new Status_Statement( "EntityNotFound", InnerEnum.EntityNotFound, ClientError, "The statement refers to a non-existent entity." );
		  public static readonly Status_Statement PropertyNotFound = new Status_Statement( "PropertyNotFound", InnerEnum.PropertyNotFound, ClientError, "The statement refers to a non-existent property." );
		  public static readonly Status_Statement LabelNotFound = new Status_Statement( "LabelNotFound", InnerEnum.LabelNotFound, ClientError, "The statement is referring to a label that does not exist." );
		  public static readonly Status_Statement TypeError = new Status_Statement( "TypeError", InnerEnum.TypeError, ClientError, "The statement is attempting to perform operations on values with types that are not supported by " + "the operation." );
		  public static readonly Status_Statement ArgumentError = new Status_Statement( "ArgumentError", InnerEnum.ArgumentError, ClientError, "The statement is attempting to perform operations using invalid arguments" );
		  public static readonly Status_Statement ArithmeticError = new Status_Statement( "ArithmeticError", InnerEnum.ArithmeticError, ClientError, "Invalid use of arithmetic, such as dividing by zero." );

		  // database errors
		  public static readonly Status_Statement ExecutionFailed = new Status_Statement( "ExecutionFailed", InnerEnum.ExecutionFailed, DatabaseError, "The database was unable to execute the statement." );

		  // transient errors
		  public static readonly Status_Statement ExternalResourceFailed = new Status_Statement( "ExternalResourceFailed", InnerEnum.ExternalResourceFailed, ClientError, "Access to an external resource failed" );

		  // client notifications (performance)
		  public static readonly Status_Statement CartesianProductWarning = new Status_Statement( "CartesianProductWarning", InnerEnum.CartesianProductWarning, ClientNotification, "This query builds a cartesian product between disconnected patterns." );
		  public static readonly Status_Statement DynamicPropertyWarning = new Status_Statement( "DynamicPropertyWarning", InnerEnum.DynamicPropertyWarning, ClientNotification, "Queries using dynamic properties will use neither index seeks nor index scans for those properties" );
		  public static readonly Status_Statement EagerOperatorWarning = new Status_Statement( "EagerOperatorWarning", InnerEnum.EagerOperatorWarning, ClientNotification, "The execution plan for this query contains the Eager operator, which forces all dependent data to " + "be materialized in main memory before proceeding" );
		  public static readonly Status_Statement JoinHintUnfulfillableWarning = new Status_Statement( "JoinHintUnfulfillableWarning", InnerEnum.JoinHintUnfulfillableWarning, ClientNotification, "The database was unable to plan a hinted join." );
		  public static readonly Status_Statement NoApplicableIndexWarning = new Status_Statement( "NoApplicableIndexWarning", InnerEnum.NoApplicableIndexWarning, ClientNotification, "Adding a schema index may speed up this query." );
		  public static readonly Status_Statement SuboptimalIndexForWildcardQuery = new Status_Statement( "SuboptimalIndexForWildcardQuery", InnerEnum.SuboptimalIndexForWildcardQuery, ClientNotification, "Index cannot execute wildcard query efficiently" );
		  public static readonly Status_Statement UnboundedVariableLengthPatternWarning = new Status_Statement( "UnboundedVariableLengthPatternWarning", InnerEnum.UnboundedVariableLengthPatternWarning, ClientNotification, "The provided pattern is unbounded, consider adding an upper limit to the number of node hops." );
		  public static readonly Status_Statement ExhaustiveShortestPathWarning = new Status_Statement( "ExhaustiveShortestPathWarning", InnerEnum.ExhaustiveShortestPathWarning, ClientNotification, "Exhaustive shortest path has been planned for your query that means that shortest path graph " + "algorithm might not be used to find the shortest path. Hence an exhaustive enumeration of all paths " + "might be used in order to find the requested shortest path." );

		  // client notifications (not supported/deprecated)
		  public static readonly Status_Statement PlannerUnavailableWarning = new Status_Statement( "PlannerUnavailableWarning", InnerEnum.PlannerUnavailableWarning, ClientNotification, "The RULE planner is not available in the current CYPHER version, the query has been run by an older " + "CYPHER version." );
		  public static readonly Status_Statement PlannerUnsupportedWarning = new Status_Statement( "PlannerUnsupportedWarning", InnerEnum.PlannerUnsupportedWarning, ClientNotification, "This query is not supported by the COST planner." );
		  public static readonly Status_Statement RuntimeUnsupportedWarning = new Status_Statement( "RuntimeUnsupportedWarning", InnerEnum.RuntimeUnsupportedWarning, ClientNotification, "This query is not supported by the chosen runtime." );
		  public static readonly Status_Statement FeatureDeprecationWarning = new Status_Statement( "FeatureDeprecationWarning", InnerEnum.FeatureDeprecationWarning, ClientNotification, "This feature is deprecated and will be removed in future versions." );
		  public static readonly Status_Statement ExperimentalFeature = new Status_Statement( "ExperimentalFeature", InnerEnum.ExperimentalFeature, ClientNotification, "This feature is experimental and should not be used in production systems." );
		  public static readonly Status_Statement JoinHintUnsupportedWarning = new Status_Statement( "JoinHintUnsupportedWarning", InnerEnum.JoinHintUnsupportedWarning, ClientNotification, "Queries with join hints are not supported by the RULE planner." );

		  // client notifications (unknown tokens)
		  public static readonly Status_Statement UnknownLabelWarning = new Status_Statement( "UnknownLabelWarning", InnerEnum.UnknownLabelWarning, ClientNotification, "The provided label is not in the database." );
		  public static readonly Status_Statement UnknownRelationshipTypeWarning = new Status_Statement( "UnknownRelationshipTypeWarning", InnerEnum.UnknownRelationshipTypeWarning, ClientNotification, "The provided relationship type is not in the database." );
		  public static readonly Status_Statement UnknownPropertyKeyWarning = new Status_Statement( "UnknownPropertyKeyWarning", InnerEnum.UnknownPropertyKeyWarning, ClientNotification, "The provided property key is not in the database" );
		  public static readonly Status_Statement CreateUniqueUnavailableWarning = new Status_Statement( "CreateUniqueUnavailableWarning", InnerEnum.CreateUniqueUnavailableWarning, ClientNotification, "CREATE UNIQUE is not available in the current CYPHER version, the query has been run by an older " + "CYPHER version." );

		  private static readonly IList<Status_Statement> valueList = new List<Status_Statement>();

		  static Status_Statement()
		  {
			  valueList.Add( SyntaxError );
			  valueList.Add( SemanticError );
			  valueList.Add( ParameterMissing );
			  valueList.Add( ConstraintVerificationFailed );
			  valueList.Add( EntityNotFound );
			  valueList.Add( PropertyNotFound );
			  valueList.Add( LabelNotFound );
			  valueList.Add( TypeError );
			  valueList.Add( ArgumentError );
			  valueList.Add( ArithmeticError );
			  valueList.Add( ExecutionFailed );
			  valueList.Add( ExternalResourceFailed );
			  valueList.Add( CartesianProductWarning );
			  valueList.Add( DynamicPropertyWarning );
			  valueList.Add( EagerOperatorWarning );
			  valueList.Add( JoinHintUnfulfillableWarning );
			  valueList.Add( NoApplicableIndexWarning );
			  valueList.Add( SuboptimalIndexForWildcardQuery );
			  valueList.Add( UnboundedVariableLengthPatternWarning );
			  valueList.Add( ExhaustiveShortestPathWarning );
			  valueList.Add( PlannerUnavailableWarning );
			  valueList.Add( PlannerUnsupportedWarning );
			  valueList.Add( RuntimeUnsupportedWarning );
			  valueList.Add( FeatureDeprecationWarning );
			  valueList.Add( ExperimentalFeature );
			  valueList.Add( JoinHintUnsupportedWarning );
			  valueList.Add( UnknownLabelWarning );
			  valueList.Add( UnknownRelationshipTypeWarning );
			  valueList.Add( UnknownPropertyKeyWarning );
			  valueList.Add( CreateUniqueUnavailableWarning );
		  }

		  public enum InnerEnum
		  {
			  SyntaxError,
			  SemanticError,
			  ParameterMissing,
			  ConstraintVerificationFailed,
			  EntityNotFound,
			  PropertyNotFound,
			  LabelNotFound,
			  TypeError,
			  ArgumentError,
			  ArithmeticError,
			  ExecutionFailed,
			  ExternalResourceFailed,
			  CartesianProductWarning,
			  DynamicPropertyWarning,
			  EagerOperatorWarning,
			  JoinHintUnfulfillableWarning,
			  NoApplicableIndexWarning,
			  SuboptimalIndexForWildcardQuery,
			  UnboundedVariableLengthPatternWarning,
			  ExhaustiveShortestPathWarning,
			  PlannerUnavailableWarning,
			  PlannerUnsupportedWarning,
			  RuntimeUnsupportedWarning,
			  FeatureDeprecationWarning,
			  ExperimentalFeature,
			  JoinHintUnsupportedWarning,
			  UnknownLabelWarning,
			  UnknownRelationshipTypeWarning,
			  UnknownPropertyKeyWarning,
			  CreateUniqueUnavailableWarning
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Statement( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Statement> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Statement valueOf( string name )
		 {
			 foreach ( Status_Statement enumInstance in Status_Statement.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_Schema : Status
	 {
		  // client errors
		  public static readonly Status_Schema RepeatedPropertyInCompositeSchema = new Status_Schema( "RepeatedPropertyInCompositeSchema", InnerEnum.RepeatedPropertyInCompositeSchema, ClientError, "Unable to create index or constraint because schema had a repeated property." );
		  public static readonly Status_Schema RepeatedLabelInSchema = new Status_Schema( "RepeatedLabelInSchema", InnerEnum.RepeatedLabelInSchema, ClientError, "Unable to create index or constraint because schema had a repeated label." );
		  public static readonly Status_Schema RepeatedRelationshipTypeInSchema = new Status_Schema( "RepeatedRelationshipTypeInSchema", InnerEnum.RepeatedRelationshipTypeInSchema, ClientError, "Unable to create index or constraint because schema had a repeated relationship type." );
		  public static readonly Status_Schema ConstraintAlreadyExists = new Status_Schema( "ConstraintAlreadyExists", InnerEnum.ConstraintAlreadyExists, ClientError, "Unable to perform operation because it would clash with a pre-existing constraint." );
		  public static readonly Status_Schema ConstraintNotFound = new Status_Schema( "ConstraintNotFound", InnerEnum.ConstraintNotFound, ClientError, "The request (directly or indirectly) referred to a constraint that does not exist." );
		  public static readonly Status_Schema ConstraintValidationFailed = new Status_Schema( "ConstraintValidationFailed", InnerEnum.ConstraintValidationFailed, ClientError, "A constraint imposed by the database was violated." );
		  public static readonly Status_Schema ConstraintVerificationFailed = new Status_Schema( "ConstraintVerificationFailed", InnerEnum.ConstraintVerificationFailed, ClientError, "Unable to create constraint because data that exists in the database violates it." );
		  public static readonly Status_Schema IndexAlreadyExists = new Status_Schema( "IndexAlreadyExists", InnerEnum.IndexAlreadyExists, ClientError, "Unable to perform operation because it would clash with a pre-existing index." );
		  public static readonly Status_Schema IndexNotFound = new Status_Schema( "IndexNotFound", InnerEnum.IndexNotFound, ClientError, "The request (directly or indirectly) referred to an index that does not exist." );
		  public static readonly Status_Schema IndexNotApplicable = new Status_Schema( "IndexNotApplicable", InnerEnum.IndexNotApplicable, ClientError, "The request did not contain the properties required by the index." );
		  public static readonly Status_Schema ForbiddenOnConstraintIndex = new Status_Schema( "ForbiddenOnConstraintIndex", InnerEnum.ForbiddenOnConstraintIndex, ClientError, "A requested operation can not be performed on the specified index because the index is part of a " + "constraint. If you want to drop the index, for instance, you must drop the constraint." );
		  public static readonly Status_Schema TokenNameError = new Status_Schema( "TokenNameError", InnerEnum.TokenNameError, ClientError, "A token name, such as a label, relationship type or property key, used is not valid. Tokens cannot " + "be empty strings and cannot be null." );

		  // database errors
		  public static readonly Status_Schema ConstraintCreationFailed = new Status_Schema( "ConstraintCreationFailed", InnerEnum.ConstraintCreationFailed, DatabaseError, "Creating a requested constraint failed." );
		  public static readonly Status_Schema ConstraintDropFailed = new Status_Schema( "ConstraintDropFailed", InnerEnum.ConstraintDropFailed, DatabaseError, "The database failed to drop a requested constraint." );
		  public static readonly Status_Schema IndexCreationFailed = new Status_Schema( "IndexCreationFailed", InnerEnum.IndexCreationFailed, DatabaseError, "Failed to create an index." );
		  public static readonly Status_Schema IndexDropFailed = new Status_Schema( "IndexDropFailed", InnerEnum.IndexDropFailed, DatabaseError, "The database failed to drop a requested index." );
		  public static readonly Status_Schema LabelAccessFailed = new Status_Schema( "LabelAccessFailed", InnerEnum.LabelAccessFailed, DatabaseError, "The request accessed a label that did not exist." );
		  public static readonly Status_Schema LabelLimitReached = new Status_Schema( "LabelLimitReached", InnerEnum.LabelLimitReached, DatabaseError, "The maximum number of labels supported has been reached, no more labels can be created." );
		  public static readonly Status_Schema PropertyKeyAccessFailed = new Status_Schema( "PropertyKeyAccessFailed", InnerEnum.PropertyKeyAccessFailed, DatabaseError, "The request accessed a property that does not exist." );
		  public static readonly Status_Schema RelationshipTypeAccessFailed = new Status_Schema( "RelationshipTypeAccessFailed", InnerEnum.RelationshipTypeAccessFailed, DatabaseError, "The request accessed a relationship type that does not exist." );
		  public static readonly Status_Schema SchemaRuleAccessFailed = new Status_Schema( "SchemaRuleAccessFailed", InnerEnum.SchemaRuleAccessFailed, DatabaseError, "The request referred to a schema rule that does not exist." );
		  public static readonly Status_Schema SchemaRuleDuplicateFound = new Status_Schema( "SchemaRuleDuplicateFound", InnerEnum.SchemaRuleDuplicateFound, DatabaseError, "The request referred to a schema rule that is defined multiple times." );

		  // transient errors
		  public static readonly Status_Schema SchemaModifiedConcurrently = new Status_Schema( "SchemaModifiedConcurrently", InnerEnum.SchemaModifiedConcurrently, TransientError, "The database schema was modified while this transaction was running, the transaction should be " + "retried." );

		  private static readonly IList<Status_Schema> valueList = new List<Status_Schema>();

		  static Status_Schema()
		  {
			  valueList.Add( RepeatedPropertyInCompositeSchema );
			  valueList.Add( RepeatedLabelInSchema );
			  valueList.Add( RepeatedRelationshipTypeInSchema );
			  valueList.Add( ConstraintAlreadyExists );
			  valueList.Add( ConstraintNotFound );
			  valueList.Add( ConstraintValidationFailed );
			  valueList.Add( ConstraintVerificationFailed );
			  valueList.Add( IndexAlreadyExists );
			  valueList.Add( IndexNotFound );
			  valueList.Add( IndexNotApplicable );
			  valueList.Add( ForbiddenOnConstraintIndex );
			  valueList.Add( TokenNameError );
			  valueList.Add( ConstraintCreationFailed );
			  valueList.Add( ConstraintDropFailed );
			  valueList.Add( IndexCreationFailed );
			  valueList.Add( IndexDropFailed );
			  valueList.Add( LabelAccessFailed );
			  valueList.Add( LabelLimitReached );
			  valueList.Add( PropertyKeyAccessFailed );
			  valueList.Add( RelationshipTypeAccessFailed );
			  valueList.Add( SchemaRuleAccessFailed );
			  valueList.Add( SchemaRuleDuplicateFound );
			  valueList.Add( SchemaModifiedConcurrently );
		  }

		  public enum InnerEnum
		  {
			  RepeatedPropertyInCompositeSchema,
			  RepeatedLabelInSchema,
			  RepeatedRelationshipTypeInSchema,
			  ConstraintAlreadyExists,
			  ConstraintNotFound,
			  ConstraintValidationFailed,
			  ConstraintVerificationFailed,
			  IndexAlreadyExists,
			  IndexNotFound,
			  IndexNotApplicable,
			  ForbiddenOnConstraintIndex,
			  TokenNameError,
			  ConstraintCreationFailed,
			  ConstraintDropFailed,
			  IndexCreationFailed,
			  IndexDropFailed,
			  LabelAccessFailed,
			  LabelLimitReached,
			  PropertyKeyAccessFailed,
			  RelationshipTypeAccessFailed,
			  SchemaRuleAccessFailed,
			  SchemaRuleDuplicateFound,
			  SchemaModifiedConcurrently
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Schema( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Schema> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Schema valueOf( string name )
		 {
			 foreach ( Status_Schema enumInstance in Status_Schema.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_LegacyIndex : Status
	 {
		  public static readonly Status_LegacyIndex LegacyIndexNotFound = new Status_LegacyIndex( "LegacyIndexNotFound", InnerEnum.LegacyIndexNotFound, ClientError, "The request (directly or indirectly) referred to an explicit index that does not exist." );

		  private static readonly IList<Status_LegacyIndex> valueList = new List<Status_LegacyIndex>();

		  static Status_LegacyIndex()
		  {
			  valueList.Add( LegacyIndexNotFound );
		  }

		  public enum InnerEnum
		  {
			  LegacyIndexNotFound
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_LegacyIndex( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_LegacyIndex> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_LegacyIndex valueOf( string name )
		 {
			 foreach ( Status_LegacyIndex enumInstance in Status_LegacyIndex.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_Procedure : Status
	 {
		  public static readonly Status_Procedure ProcedureRegistrationFailed = new Status_Procedure( "ProcedureRegistrationFailed", InnerEnum.ProcedureRegistrationFailed, ClientError, "The database failed to register a procedure, refer to the associated error message for details." );
		  public static readonly Status_Procedure ProcedureNotFound = new Status_Procedure( "ProcedureNotFound", InnerEnum.ProcedureNotFound, ClientError, "A request referred to a procedure that is not registered with this database instance. If you are " + "deploying custom procedures in a cluster setup, ensure all instances in the cluster have the " + "procedure jar file deployed." );
		  public static readonly Status_Procedure ProcedureCallFailed = new Status_Procedure( "ProcedureCallFailed", InnerEnum.ProcedureCallFailed, ClientError, "Failed to invoke a procedure. See the detailed error description for exact cause." );
		  public static readonly Status_Procedure TypeError = new Status_Procedure( "TypeError", InnerEnum.TypeError, ClientError, "A procedure is using or receiving a value of an invalid type." );
		  public static readonly Status_Procedure ProcedureTimedOut = new Status_Procedure( "ProcedureTimedOut", InnerEnum.ProcedureTimedOut, ClientError, "The procedure has not completed within the specified timeout. You may want to retry with a longer " + "timeout." );
		  public static readonly Status_Procedure ProcedureWarning = new Status_Procedure( "ProcedureWarning", InnerEnum.ProcedureWarning, ClientNotification, "The query used a procedure that generated a warning." );

		  private static readonly IList<Status_Procedure> valueList = new List<Status_Procedure>();

		  static Status_Procedure()
		  {
			  valueList.Add( ProcedureRegistrationFailed );
			  valueList.Add( ProcedureNotFound );
			  valueList.Add( ProcedureCallFailed );
			  valueList.Add( TypeError );
			  valueList.Add( ProcedureTimedOut );
			  valueList.Add( ProcedureWarning );
		  }

		  public enum InnerEnum
		  {
			  ProcedureRegistrationFailed,
			  ProcedureNotFound,
			  ProcedureCallFailed,
			  TypeError,
			  ProcedureTimedOut,
			  ProcedureWarning
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Procedure( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Procedure> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Procedure valueOf( string name )
		 {
			 foreach ( Status_Procedure enumInstance in Status_Procedure.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_Security : Status
	 {
		  // client
		  public static readonly Status_Security CredentialsExpired = new Status_Security( "CredentialsExpired", InnerEnum.CredentialsExpired, ClientError, "The credentials have expired and need to be updated." );
		  public static readonly Status_Security Unauthorized = new Status_Security( "Unauthorized", InnerEnum.Unauthorized, ClientError, "The client is unauthorized due to authentication failure." );
		  public static readonly Status_Security AuthenticationRateLimit = new Status_Security( "AuthenticationRateLimit", InnerEnum.AuthenticationRateLimit, ClientError, "The client has provided incorrect authentication details too many times in a row." );
		  public static readonly Status_Security ModifiedConcurrently = new Status_Security( "ModifiedConcurrently", InnerEnum.ModifiedConcurrently, TransientError, "The user was modified concurrently to this request." );
		  public static readonly Status_Security EncryptionRequired = new Status_Security( "EncryptionRequired", InnerEnum.EncryptionRequired, ClientError, "A TLS encrypted connection is required." );
		  public static readonly Status_Security Forbidden = new Status_Security( "Forbidden", InnerEnum.Forbidden, ClientError, "An attempt was made to perform an unauthorized action." );
		  public static readonly Status_Security AuthorizationExpired = new Status_Security( "AuthorizationExpired", InnerEnum.AuthorizationExpired, ClientError, "The stored authorization info has expired. Please reconnect." );
		  public static readonly Status_Security AuthProviderTimeout = new Status_Security( "AuthProviderTimeout", InnerEnum.AuthProviderTimeout, TransientError, "An auth provider request timed out." );
		  public static readonly Status_Security AuthProviderFailed = new Status_Security( "AuthProviderFailed", InnerEnum.AuthProviderFailed, TransientError, "An auth provider request failed." );

		  private static readonly IList<Status_Security> valueList = new List<Status_Security>();

		  static Status_Security()
		  {
			  valueList.Add( CredentialsExpired );
			  valueList.Add( Unauthorized );
			  valueList.Add( AuthenticationRateLimit );
			  valueList.Add( ModifiedConcurrently );
			  valueList.Add( EncryptionRequired );
			  valueList.Add( Forbidden );
			  valueList.Add( AuthorizationExpired );
			  valueList.Add( AuthProviderTimeout );
			  valueList.Add( AuthProviderFailed );
		  }

		  public enum InnerEnum
		  {
			  CredentialsExpired,
			  Unauthorized,
			  AuthenticationRateLimit,
			  ModifiedConcurrently,
			  EncryptionRequired,
			  Forbidden,
			  AuthorizationExpired,
			  AuthProviderTimeout,
			  AuthProviderFailed
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Security( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Security> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Security valueOf( string name )
		 {
			 foreach ( Status_Security enumInstance in Status_Security.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_General : Status
	 {
		  // client errors
		  public static readonly Status_General InvalidArguments = new Status_General( "InvalidArguments", InnerEnum.InvalidArguments, ClientError, "The request contained fields that were empty or are not allowed." );
		  public static readonly Status_General ForbiddenOnReadOnlyDatabase = new Status_General( "ForbiddenOnReadOnlyDatabase", InnerEnum.ForbiddenOnReadOnlyDatabase, ClientError, "This is a read only database, writing or modifying the database is not allowed." );

		  // database errors
		  public static readonly Status_General IndexCorruptionDetected = new Status_General( "IndexCorruptionDetected", InnerEnum.IndexCorruptionDetected, DatabaseError, "The request (directly or indirectly) referred to an index that is in a failed state. The index " + "needs to be dropped and recreated manually." );
		  public static readonly Status_General SchemaCorruptionDetected = new Status_General( "SchemaCorruptionDetected", InnerEnum.SchemaCorruptionDetected, DatabaseError, "A malformed schema rule was encountered. Please contact your support representative." );
		  public static readonly Status_General StorageDamageDetected = new Status_General( "StorageDamageDetected", InnerEnum.StorageDamageDetected, DatabaseError, "Expected set of files not found on disk. Please restore from backup." );
		  public static readonly Status_General UnknownError = new Status_General( "UnknownError", InnerEnum.UnknownError, DatabaseError, "An unknown error occurred." );
		  public static readonly Status_General OutOfMemoryError = new Status_General( "OutOfMemoryError", InnerEnum.OutOfMemoryError, TransientError, "There is not enough memory to perform the current task. Please try increasing " + "'dbms.memory.heap.max_size' in the neo4j configuration (normally in 'conf/neo4j.conf' or, if you " + "you are using Neo4j Desktop, found through the user interface) or if you are running an embedded " + "installation increase the heap by using '-Xmx' command line flag, and then restart the database." );
		  public static readonly Status_General StackOverFlowError = new Status_General( "StackOverFlowError", InnerEnum.StackOverFlowError, TransientError, "There is not enough stack size to perform the current task. This is generally considered to be a " + "database error, so please contact Neo4j support. You could try increasing the stack size: " + "for example to set the stack size to 2M, add `dbms.jvm.additional=-Xss2M' to " + "in the neo4j configuration (normally in 'conf/neo4j.conf' or, if you are using " + "Neo4j Desktop, found through the user interface) or if you are running an embedded installation " + "just add -Xss2M as command line flag." );

		  // transient errors
		  public static readonly Status_General DatabaseUnavailable = new Status_General( "DatabaseUnavailable", InnerEnum.DatabaseUnavailable, TransientError, "The database is not currently available to serve your request, refer to the database logs for more " + "details. Retrying your request at a later time may succeed." );

		  private static readonly IList<Status_General> valueList = new List<Status_General>();

		  static Status_General()
		  {
			  valueList.Add( InvalidArguments );
			  valueList.Add( ForbiddenOnReadOnlyDatabase );
			  valueList.Add( IndexCorruptionDetected );
			  valueList.Add( SchemaCorruptionDetected );
			  valueList.Add( StorageDamageDetected );
			  valueList.Add( UnknownError );
			  valueList.Add( OutOfMemoryError );
			  valueList.Add( StackOverFlowError );
			  valueList.Add( DatabaseUnavailable );
		  }

		  public enum InnerEnum
		  {
			  InvalidArguments,
			  ForbiddenOnReadOnlyDatabase,
			  IndexCorruptionDetected,
			  SchemaCorruptionDetected,
			  StorageDamageDetected,
			  UnknownError,
			  OutOfMemoryError,
			  StackOverFlowError,
			  DatabaseUnavailable
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_General( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_General> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_General valueOf( string name )
		 {
			 foreach ( Status_General enumInstance in Status_General.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Status_Cluster : Status
	 {
		  // transient errors
		  public static readonly Status_Cluster NoLeaderAvailable = new Status_Cluster( "NoLeaderAvailable", InnerEnum.NoLeaderAvailable, TransientError, "No leader available at the moment. Retrying your request at a later time may succeed." );

		  public static readonly Status_Cluster ReplicationFailure = new Status_Cluster( "ReplicationFailure", InnerEnum.ReplicationFailure, TransientError, "Replication failure." );

		  public static readonly Status_Cluster NotALeader = new Status_Cluster( "NotALeader", InnerEnum.NotALeader, ClientError, "The request cannot be processed by this server. Write requests can only be processed by the leader." );

		  private static readonly IList<Status_Cluster> valueList = new List<Status_Cluster>();

		  static Status_Cluster()
		  {
			  valueList.Add( NoLeaderAvailable );
			  valueList.Add( ReplicationFailure );
			  valueList.Add( NotALeader );
		  }

		  public enum InnerEnum
		  {
			  NoLeaderAvailable,
			  ReplicationFailure,
			  NotALeader
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final Status_Code;

		  public Status_Code Code()
		  {
				return CodeConflict;
		  }

		  internal Status_Cluster( string name, InnerEnum innerEnum, Status_Classification classification, string description )
		  {
				this.CodeConflict = new Status_Code( classification, this, description );

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		 public static IList<Status_Cluster> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static Status_Cluster valueOf( string name )
		 {
			 foreach ( Status_Cluster enumInstance in Status_Cluster.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public class Status_Code
	 {
		  public static ICollection<Status> All()
		  {
				ICollection<Status> result = new List<Status>();
				foreach ( Type child in typeof( Status ).GetNestedTypes( BindingFlags.Public | BindingFlags.NonPublic ) )
				{
					 if ( child.IsEnum && child.IsAssignableFrom( typeof( Status ) ) )
					 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Class statusType = (Class) child;
						  Type statusType = ( Type ) child;
						  Collections.addAll( result, statusType.EnumConstants );
					 }
				}
				return result;
		  }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly Status_Classification ClassificationConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly string DescriptionConflict;
		  internal readonly string Category;
		  internal readonly string Title;

		  internal Status_Code<C>( Status_Classification classification, C categoryAndTitle, string description ) where C : Enum<C>, Status
		  {
				this.ClassificationConflict = classification;
				this.Category = categoryAndTitle.DeclaringClass.SimpleName;
				this.Title = categoryAndTitle.name();

				this.DescriptionConflict = description;
		  }

		  public override string ToString()
		  {
				return "Status.Code[" + Serialize() + "]";
		  }

		  /// <summary>
		  /// The portable, serialized status code. This will always be in the format:
		  /// 
		  /// <pre>
		  /// Neo.[Classification].[Category].[Title]
		  /// </pre>
		  /// </summary>
		  public string Serialize()
		  {
				return format( "Neo.%s.%s.%s", ClassificationConflict, Category, Title );
		  }

		  public string Description()
		  {
				return DescriptionConflict;
		  }

		  public virtual Status_Classification Classification()
		  {
				return ClassificationConflict;
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}

				Status_Code code = ( Status_Code ) o;

				return Category.Equals( code.Category ) && ClassificationConflict == code.ClassificationConflict && Title.Equals( code.Title );
		  }

		  public override int GetHashCode()
		  {
				int result = ClassificationConflict.GetHashCode();
				result = 31 * result + Category.GetHashCode();
				result = 31 * result + Title.GetHashCode();
				return result;
		  }
	 }

	 public enum Status_Classification
	 {
		  /// <summary>
		  /// The Client sent a bad request - changing the request might yield a successful outcome. </summary>
		  ClientError( TransactionEffect.ROLLBACK, "The Client sent a bad request - changing the request might yield a successful outcome." ),
		  /// <summary>
		  /// There are notifications about the request sent by the client. </summary>
		  ClientNotification( TransactionEffect.NONE, "There are notifications about the request sent by the client." ),

		  /// <summary>
		  /// The database cannot service the request right now, retrying later might yield a successful outcome. </summary>
		  TransientError( TransactionEffect.ROLLBACK, "The database cannot service the request right now, retrying later might yield a successful outcome. " ),

		  // Implementation note: These are a sharp tool, database error signals
		  // that something is *seriously* wrong, and will prompt the user to send
		  // an error report back to us. Only use this if the code path you are
		  // at would truly indicate the database is in a broken or bug-induced state.
		  /// <summary>
		  /// The database failed to service the request. </summary>
		  DatabaseError( TransactionEffect.ROLLBACK, "The database failed to service the request. " );

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		  enum TransactionEffect
	//	  {
	//			ROLLBACK,
	//			NONE,
	//	  }

	 }
	 public interface Status_HasStatus
	 {
		  Status Status();
	 }
}