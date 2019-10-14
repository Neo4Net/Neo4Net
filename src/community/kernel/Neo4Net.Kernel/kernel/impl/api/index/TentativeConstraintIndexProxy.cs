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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using ConstraintValidationException = Neo4Net.Internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using DelegatingIndexUpdater = Neo4Net.Kernel.Impl.Api.index.updater.DelegatingIndexUpdater;
	using DeferredConflictCheckingIndexUpdater = Neo4Net.Kernel.Impl.Index.Schema.DeferredConflictCheckingIndexUpdater;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;

	/// <summary>
	/// What is a tentative constraint index proxy? Well, the way we build uniqueness constraints is as follows:
	/// <ol>
	/// <li>Begin a transaction T, which will be the "parent" transaction in this process</li>
	/// <li>Execute a mini transaction Tt which will create the index rule to start the index population</li>
	/// <li>In T: Sit and wait for the index to be built</li>
	/// <li>In T: Create the constraint rule and connect the two</li>
	/// </ol>
	/// 
	/// The fully populated index flips to a tentative index. The reason for that is to guard for incoming transactions
	/// that gets applied.
	/// Such incoming transactions have potentially been verified on another instance with a slightly dated view
	/// of the schema and has furthermore made it through some additional checks on this instance since transaction T
	/// hasn't yet fully committed. Transaction data gets applied to the neo store first and the index second, so at
	/// the point where the applying transaction sees that it violates the constraint it has already modified the store and
	/// cannot back out. However the constraint transaction T can. So a violated constraint while
	/// in tentative mode does not fail the transaction violating the constraint, but keeps the failure around and will
	/// eventually fail T instead.
	/// </summary>
	public class TentativeConstraintIndexProxy : AbstractDelegatingIndexProxy
	{
		 private readonly FlippableIndexProxy _flipper;
		 private readonly OnlineIndexProxy _target;
		 private readonly ICollection<IndexEntryConflictException> _failures = new CopyOnWriteArrayList<IndexEntryConflictException>();

		 internal TentativeConstraintIndexProxy( FlippableIndexProxy flipper, OnlineIndexProxy target )
		 {
			  this._flipper = flipper;
			  this._target = target;
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  switch ( mode.innerEnumValue )
			  {
					case Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.ONLINE:
						 return new DelegatingIndexUpdaterAnonymousInnerClass( this );

					case Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.RECOVERY:
						 return base.NewUpdater( mode );

					default:
						 throw new System.ArgumentException( "Unsupported update mode: " + mode );

			  }
		 }

		 private class DelegatingIndexUpdaterAnonymousInnerClass : DelegatingIndexUpdater
		 {
			 private readonly TentativeConstraintIndexProxy _outerInstance;

			 public DelegatingIndexUpdaterAnonymousInnerClass( TentativeConstraintIndexProxy outerInstance ) : base( new DeferredConflictCheckingIndexUpdater( outerInstance.target.Accessor.newUpdater( mode ), outerInstance.target.newReader, outerInstance.target.Descriptor ) )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void process<T1>( IndexEntryUpdate<T1> update )
			 {
				  try
				  {
						@delegate.process( update );
				  }
				  catch ( IndexEntryConflictException conflict )
				  {
						_outerInstance.failures.Add( conflict );
				  }
			 }

			 public override void close()
			 {
				  try
				  {
						@delegate.close();
				  }
				  catch ( IndexEntryConflictException conflict )
				  {
						_outerInstance.failures.Add( conflict );
				  }
			 }
		 }

		 public override InternalIndexState State
		 {
			 get
			 {
				  return _failures.Count == 0 ? InternalIndexState.POPULATING : InternalIndexState.FAILED;
			 }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[target:" + _target + "]";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexReader newReader() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexReader NewReader()
		 {
			  throw new IndexNotFoundKernelException( Descriptor + " is still populating" );
		 }

		 protected internal override IndexProxy Delegate
		 {
			 get
			 {
				  return _target;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor accessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor accessor )
		 {
			  // If we've seen constraint violation failures in here when updates came in then fail immediately with those
			  if ( _failures.Count > 0 )
			  {
					IEnumerator<IndexEntryConflictException> failureIterator = _failures.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IndexEntryConflictException conflict = failureIterator.next();
					failureIterator.forEachRemaining( conflict.addSuppressed );
					throw conflict;
			  }

			  // Otherwise consolidate the usual verification
			  base.VerifyDeferredConstraints( accessor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validate() throws org.neo4j.kernel.api.exceptions.schema.UniquePropertyValueValidationException
		 public override void Validate()
		 {
			  if ( _failures.Count > 0 )
			  {
					SchemaDescriptor descriptor = Descriptor.schema();
					throw new UniquePropertyValueValidationException(ConstraintDescriptorFactory.uniqueForSchema(descriptor), ConstraintValidationException.Phase.VERIFICATION, new HashSet<>(_failures)
						);
			  }
		 }

		 public override void Activate()
		 {
			  if ( _failures.Count == 0 )
			  {
					_flipper.flipTo( _target );
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Trying to activate failed index, should have checked the failures earlier..." );
			  }
		 }
	}

}