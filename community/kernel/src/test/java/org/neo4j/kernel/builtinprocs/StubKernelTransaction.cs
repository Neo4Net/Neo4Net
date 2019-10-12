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
namespace Org.Neo4j.Kernel.builtinprocs
{

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
	using ClockContext = Org.Neo4j.Kernel.Impl.Api.ClockContext;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;

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

		 public override void RegisterCloseListener( Org.Neo4j.Kernel.api.KernelTransaction_CloseListener listener )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override Org.Neo4j.@internal.Kernel.Api.Transaction_Type TransactionType()
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

		 public override Org.Neo4j.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
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