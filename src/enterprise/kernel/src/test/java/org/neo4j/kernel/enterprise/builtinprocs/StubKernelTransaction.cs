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
namespace Neo4Net.Kernel.enterprise.builtinprocs
{

	using Answers = org.mockito.Answers;
	using CursorFactory = Neo4Net.Kernel.Api.Internal.CursorFactory;
	using ExecutionStatistics = Neo4Net.Kernel.Api.Internal.ExecutionStatistics;
	using ExplicitIndexRead = Neo4Net.Kernel.Api.Internal.ExplicitIndexRead;
	using ExplicitIndexWrite = Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite;
	using Locks = Neo4Net.Kernel.Api.Internal.Locks;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using Procedures = Neo4Net.Kernel.Api.Internal.Procedures;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using SchemaRead = Neo4Net.Kernel.Api.Internal.SchemaRead;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using Token = Neo4Net.Kernel.Api.Internal.Token;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using Write = Neo4Net.Kernel.Api.Internal.Write;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using AuthSubject = Neo4Net.Kernel.Api.Internal.security.AuthSubject;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using StubStatement = Neo4Net.Kernel.builtinprocs.StubStatement;
	using ClockContext = Neo4Net.Kernel.Impl.Api.ClockContext;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;

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

		 public override void RegisterCloseListener( Neo4Net.Kernel.api.KernelTransaction_CloseListener listener )
		 {
		 }

		 public override Neo4Net.Kernel.Api.Internal.Transaction_Type TransactionType()
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

		 public override Neo4Net.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
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