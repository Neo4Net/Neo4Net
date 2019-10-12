using System;
using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.store.kvstore
{

	using Org.Neo4j.Helpers.Collection;

	internal abstract class RotationState<Key> : ProgressiveState<Key>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract ProgressiveState<Key> rotate(boolean force, RotationStrategy strategy, RotationTimerFactory timerFactory, System.Action<Headers.Builder> headersUpdater) throws java.io.IOException;
		 internal abstract ProgressiveState<Key> Rotate( bool force, RotationStrategy strategy, RotationTimerFactory timerFactory, System.Action<Headers.Builder> headersUpdater );

		 internal override string StateName()
		 {
			  return "rotating";
		 }

		 internal abstract long RotationVersion();

		 /// <summary>
		 /// Marks state as failed and returns the state as it were before this state.
		 /// </summary>
		 /// <returns> previous state. </returns>
		 internal abstract ProgressiveState<Key> MarkAsFailed();

		 internal sealed class Rotation<Key> : RotationState<Key>
		 {
			  internal readonly ActiveState<Key> PreState;
			  internal readonly PrototypeState<Key> PostState;
			  internal readonly long Threshold;
			  internal bool Failed;

			  internal Rotation( ActiveState<Key> preState, PrototypeState<Key> postState, long version )
			  {
					this.PreState = preState;
					this.PostState = postState;
					this.Threshold = version;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ActiveState<Key> rotate(boolean force, RotationStrategy strategy, RotationTimerFactory timerFactory, System.Action<Headers.Builder> headersUpdater) throws java.io.IOException
			  internal override ActiveState<Key> Rotate( bool force, RotationStrategy strategy, RotationTimerFactory timerFactory, System.Action<Headers.Builder> headersUpdater )
			  {
					if ( !force )
					{
						 RotationTimerFactory.RotationTimer rotationTimer = timerFactory.CreateTimer();
						 for ( long expected = Threshold - PreState.store.version(), sleep = 10; PreState.applied() < expected; sleep = Math.Min(sleep * 2, 100) )
						 {
							  if ( rotationTimer.TimedOut )
							  {
									throw new RotationTimeoutException( Threshold, PreState.store.version(), rotationTimer.ElapsedTimeMillis );
							  }
							  try
							  {
									Thread.Sleep( sleep );
							  }
							  catch ( InterruptedException e )
							  {
									throw ( InterruptedIOException ) ( new InterruptedIOException( "Rotation was interrupted." ) ).initCause( e );
							  }
						 }
					}
					Pair<File, KeyValueStoreFile> next = strategy.Next( File(), UpdateHeaders(headersUpdater), KeyFormat().filter(PreState.dataProvider()) );
					return PostState.create( ReadableState.Store( PreState.keyFormat(), next.Other() ), next.First(), PreState.versionContextSupplier );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			  public override void Close()
			  {
					if ( !Failed )
					{
						 // We can't just close the pre-state (the only good state right now) if the rotation failed.
						 PreState.Dispose();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ProgressiveState<Key> stop() throws java.io.IOException
			  internal override ProgressiveState<Key> Stop()
			  {
					if ( Failed )
					{
						 // failed to rotate allow for stopping no matter what
						 return PreState;
					}
					return base.Stop();
			  }

			  internal override long RotationVersion()
			  {
					return Threshold;
			  }

			  internal Headers UpdateHeaders( System.Action<Headers.Builder> headersUpdater )
			  {
					Headers.Builder builder = new Headers.Builder( Headers.Copy( PreState.headers() ) );
					headersUpdater( builder );
					return builder.Headers();
			  }

			  protected internal override Optional<EntryUpdater<Key>> OptionalUpdater( long version, Lock @lock )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EntryUpdater<Key> post = postState.updater(version, lock);
					EntryUpdater<Key> post = PostState.updater( version, @lock );
					if ( version <= Threshold )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EntryUpdater<Key> pre = preState.updater(version, lock);
						 EntryUpdater<Key> pre = PreState.updater( version, @lock );
						 return ( new EntryUpdaterAnonymousInnerClass( this, @lock, post, pre ) );
					}
					else
					{
						 return post;
					}
			  }

			  private class EntryUpdaterAnonymousInnerClass : EntryUpdater<Key>
			  {
				  private readonly Rotation<Key> _outerInstance;

				  private Org.Neo4j.Kernel.impl.store.kvstore.EntryUpdater<Key> _post;
				  private Org.Neo4j.Kernel.impl.store.kvstore.EntryUpdater<Key> _pre;

				  public EntryUpdaterAnonymousInnerClass( Rotation<Key> outerInstance, Lock @lock, Org.Neo4j.Kernel.impl.store.kvstore.EntryUpdater<Key> post, Org.Neo4j.Kernel.impl.store.kvstore.EntryUpdater<Key> pre ) : base( @lock )
				  {
					  this.outerInstance = outerInstance;
					  this._post = post;
					  this._pre = pre;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(Key key, ValueUpdate update) throws java.io.IOException
				  public override void apply( Key key, ValueUpdate update )
				  {
						// Apply to the postState first, so that if the postState needs to read the state from the preState
						// it will read the value prior to this update, then subsequent updates to the postState will not
						// have to read from preState, ensuring that each update is applied exactly once to both preState
						// and postState, which together with the commutativity of updates ensures consistent outcomes.
						_post.apply( key, update );
						_pre.apply( key, update );
				  }

				  public override void close()
				  {
						_post.close();
						_pre.close();
						base.close();
				  }
			  }

			  protected internal override File File()
			  {
					return PreState.file();
			  }

			  protected internal override long StoredVersion()
			  {
					return PreState.storedVersion();
			  }

			  protected internal override KeyFormat<Key> KeyFormat()
			  {
					return PreState.keyFormat();
			  }

			  protected internal override Headers Headers()
			  {
					return PreState.headers();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected DataProvider dataProvider() throws java.io.IOException
			  protected internal override DataProvider DataProvider()
			  {
					return PostState.dataProvider();
			  }

			  protected internal override int StoredEntryCount()
			  {
					return PostState.storedEntryCount();
			  }

			  protected internal override EntryUpdater<Key> UnsafeUpdater( Lock @lock )
			  {
					return PostState.unsafeUpdater( @lock );
			  }

			  protected internal override bool HasChanges()
			  {
					return PreState.hasChanges() || PostState.hasChanges();
			  }

			  protected internal override long Version()
			  {
					return PostState.version();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean lookup(Key key, ValueSink sink) throws java.io.IOException
			  protected internal override bool Lookup( Key key, ValueSink sink )
			  {
					return PostState.lookup( key, sink );
			  }

			  internal override ProgressiveState<Key> MarkAsFailed()
			  {
					Failed = true;
					return PreState;
			  }
		 }
	}

}