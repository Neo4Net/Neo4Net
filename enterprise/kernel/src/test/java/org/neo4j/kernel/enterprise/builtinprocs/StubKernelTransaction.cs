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
namespace Org.Neo4j.Kernel.enterprise.builtinprocs
{

	using Answers = org.mockito.Answers;
	using CursorFactory = Org.Neo4j.@internal.Kernel.Api.CursorFactory;
	using ExecutionStatistics = Org.Neo4j.@internal.Kernel.Api.ExecutionStatistics;
	using ExplicitIndexRead = Org.Neo4j.@internal.Kernel.Api.ExplicitIndexRead;
	using ExplicitIndexWrite = Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite;
	using Locks = Org.Neo4j.@internal.Kernel.Api.Locks;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using Procedures = Org.Neo4j.@internal.Kernel.Api.Procedures;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using SchemaRead = Org.Neo4j.@internal.Kernel.Api.SchemaRead;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using Token = Org.Neo4j.@internal.Kernel.Api.Token;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using TokenWrite = Org.Neo4j.@internal.Kernel.Api.TokenWrite;
	using Write = Org.Neo4j.@internal.Kernel.Api.Write;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using AuthSubject = Org.Neo4j.@internal.Kernel.Api.security.AuthSubject;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using StubStatement = Org.Neo4j.Kernel.builtinprocs.StubStatement;
	using ClockContext = Org.Neo4j.Kernel.Impl.Api.ClockContext;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class StubKernelTransaction : KernelTransaction
	{

		 internal StubKernelTransaction()
		 {
		 }

		 public override Statement AcquireStatement()
		 {
			  return new StubStatement();
		 }

		 public override IndexDescriptor IndexUniqueCreate( SchemaDescriptor schema, string provider )
		 {
			  return null;
		 }

		 public override void Success()
		 {
		 }

		 public override void Failure()
		 {
		 }

		 public virtual IDictionary<string, object> MetaData
		 {
			 get
			 {
				  return null;
			 }
			 set
			 {
			 }
		 }

		 public override Read DataRead()
		 {
			  return null;
		 }

		 public override Write DataWrite()
		 {
			  return null;
		 }

		 public override ExplicitIndexRead IndexRead()
		 {
			  return null;
		 }

		 public override ExplicitIndexWrite IndexWrite()
		 {
			  return null;
		 }

		 public override TokenRead TokenRead()
		 {
			  return null;
		 }

		 public override TokenWrite TokenWrite()
		 {
			  return null;
		 }

		 public override Token Token()
		 {
			  return null;
		 }

		 public override SchemaRead SchemaRead()
		 {
			  return null;
		 }

		 public override Procedures Procedures()
		 {
			  return null;
		 }

		 public override ExecutionStatistics ExecutionStatistics()
		 {
			  return null;
		 }

		 public override SchemaWrite SchemaWrite()
		 {
			  return null;
		 }

		 public override Locks Locks()
		 {
			  return null;
		 }

		 public override CursorFactory Cursors()
		 {
			  return null;
		 }

		 public override long CloseTransaction()
		 {
			  return 0;
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override SecurityContext SecurityContext()
		 {
			  SecurityContext securityContext = mock( typeof( SecurityContext ), Answers.RETURNS_DEEP_STUBS );
			  when( securityContext.Subject().username() ).thenReturn("testUser");
			  return securityContext;
		 }

		 public override AuthSubject SubjectOrAnonymous()
		 {
			  AuthSubject subject = mock( typeof( AuthSubject ) );
			  when( subject.Username() ).thenReturn("testUser");
			  return subject;
		 }

		 public virtual Optional<Status> ReasonIfTerminated
		 {
			 get
			 {
				  return null;
			 }
		 }

		 public virtual bool Terminated
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override void MarkForTermination( Status reason )
		 {
		 }

		 public override long LastTransactionTimestampWhenStarted()
		 {
			  return 0;
		 }

		 public override long LastTransactionIdWhenStarted()
		 {
			  return 0;
		 }

		 public override long StartTime()
		 {
			  return 1984;
		 }

		 public override long StartTimeNanos()
		 {
			  return 1984000;
		 }

		 public override long Timeout()
		 {
			  return 0;
		 }

		 public override void RegisterCloseListener( Org.Neo4j.Kernel.api.KernelTransaction_CloseListener listener )
		 {
		 }

		 public override Org.Neo4j.@internal.Kernel.Api.Transaction_Type TransactionType()
		 {
			  return null;
		 }

		 public virtual long TransactionId
		 {
			 get
			 {
				  return 8;
			 }
		 }

		 public virtual long CommitTime
		 {
			 get
			 {
				  return 0;
			 }
		 }

		 public override Org.Neo4j.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
		 {
			  return null;
		 }

		 public override NodeCursor AmbientNodeCursor()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override RelationshipScanCursor AmbientRelationshipCursor()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override PropertyCursor AmbientPropertyCursor()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }


		 public override ClockContext Clocks()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void AssertOpen()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public virtual bool SchemaTransaction
		 {
			 get
			 {
				  return false;
			 }
		 }


	}
}