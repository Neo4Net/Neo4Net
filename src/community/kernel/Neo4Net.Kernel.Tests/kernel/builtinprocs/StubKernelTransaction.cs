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
namespace Neo4Net.Kernel.builtinprocs
{

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
	using ClockContext = Neo4Net.Kernel.Impl.Api.ClockContext;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;

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
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Success()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Failure()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override Read DataRead()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override Write DataWrite()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override ExplicitIndexRead IndexRead()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override ExplicitIndexWrite IndexWrite()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override TokenRead TokenRead()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override TokenWrite TokenWrite()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override Token Token()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override SchemaRead SchemaRead()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override SchemaWrite SchemaWrite()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override Locks Locks()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override CursorFactory Cursors()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override Procedures Procedures()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override ExecutionStatistics ExecutionStatistics()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long CloseTransaction()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  throw new System.NotSupportedException( "not implemented" );
			 }
		 }

		 public override SecurityContext SecurityContext()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override AuthSubject SubjectOrAnonymous()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public virtual Optional<Status> ReasonIfTerminated
		 {
			 get
			 {
				  throw new System.NotSupportedException( "not implemented" );
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
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long LastTransactionTimestampWhenStarted()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long LastTransactionIdWhenStarted()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long StartTime()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long StartTimeNanos()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long Timeout()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void RegisterCloseListener( Neo4Net.Kernel.api.KernelTransaction_CloseListener listener )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override Neo4Net.Kernel.Api.Internal.Transaction_Type TransactionType()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public virtual long TransactionId
		 {
			 get
			 {
				  throw new System.NotSupportedException( "not implemented" );
			 }
		 }

		 public virtual long CommitTime
		 {
			 get
			 {
				  throw new System.NotSupportedException( "not implemented" );
			 }
		 }

		 public override Neo4Net.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override ClockContext Clocks()
		 {
			  throw new System.NotSupportedException( "not implemented" );
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

		 public virtual IDictionary<string, object> MetaData
		 {
			 set
			 {
				  throw new System.NotSupportedException( "not implemented" );
			 }
			 get
			 {
				  throw new System.NotSupportedException( "not implemented" );
			 }
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