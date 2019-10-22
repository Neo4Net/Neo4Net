using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using IDisposablePlus = Neo4Net.Internal.Kernel.Api.IDisposablePlus;
	using CursorFactory = Neo4Net.Internal.Kernel.Api.CursorFactory;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.FeatureToggles.flag;

	public class DefaultCursors : CursorFactory
	{
		 private readonly StorageReader _storageReader;
		 private DefaultNodeCursor _nodeCursor;
		 private DefaultRelationshipScanCursor _relationshipScanCursor;
		 private DefaultRelationshipTraversalCursor _relationshipTraversalCursor;
		 private DefaultPropertyCursor _propertyCursor;
		 private DefaultRelationshipGroupCursor _relationshipGroupCursor;
		 private DefaultNodeValueIndexCursor _nodeValueIndexCursor;
		 private DefaultNodeLabelIndexCursor _nodeLabelIndexCursor;
		 private DefaultNodeExplicitIndexCursor _nodeExplicitIndexCursor;
		 private DefaultRelationshipExplicitIndexCursor _relationshipExplicitIndexCursor;

		 private static readonly bool _debugClosing = flag( typeof( DefaultCursors ), "trackCursors", false );
		 private IList<CloseableStacktrace> _closeables = new List<CloseableStacktrace>();

		 public DefaultCursors( StorageReader storageReader )
		 {
			  this._storageReader = storageReader;
		 }

		 public override DefaultNodeCursor AllocateNodeCursor()
		 {
			  if ( _nodeCursor == null )
			  {
					return Trace( new DefaultNodeCursor( this, _storageReader.allocateNodeCursor() ) );
			  }

			  try
			  {
					return _nodeCursor;
			  }
			  finally
			  {
					_nodeCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultNodeCursor cursor )
		 {
			  if ( _nodeCursor != null )
			  {
					_nodeCursor.release();
			  }
			  _nodeCursor = cursor;
		 }

		 public override DefaultRelationshipScanCursor AllocateRelationshipScanCursor()
		 {
			  if ( _relationshipScanCursor == null )
			  {
					return Trace( new DefaultRelationshipScanCursor( this, _storageReader.allocateRelationshipScanCursor() ) );
			  }

			  try
			  {
					return _relationshipScanCursor;
			  }
			  finally
			  {
					_relationshipScanCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultRelationshipScanCursor cursor )
		 {
			  if ( _relationshipScanCursor != null )
			  {
					_relationshipScanCursor.release();
			  }
			  _relationshipScanCursor = cursor;
		 }

		 public override DefaultRelationshipTraversalCursor AllocateRelationshipTraversalCursor()
		 {
			  if ( _relationshipTraversalCursor == null )
			  {
					return Trace( new DefaultRelationshipTraversalCursor( this, _storageReader.allocateRelationshipTraversalCursor() ) );
			  }

			  try
			  {
					return _relationshipTraversalCursor;
			  }
			  finally
			  {
					_relationshipTraversalCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultRelationshipTraversalCursor cursor )
		 {
			  if ( _relationshipTraversalCursor != null )
			  {
					_relationshipTraversalCursor.release();
			  }
			  _relationshipTraversalCursor = cursor;
		 }

		 public override DefaultPropertyCursor AllocatePropertyCursor()
		 {
			  if ( _propertyCursor == null )
			  {
					return Trace( new DefaultPropertyCursor( this, _storageReader.allocatePropertyCursor() ) );
			  }

			  try
			  {
					return _propertyCursor;
			  }
			  finally
			  {
					_propertyCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultPropertyCursor cursor )
		 {
			  if ( _propertyCursor != null )
			  {
					_propertyCursor.release();
			  }
			  _propertyCursor = cursor;
		 }

		 public override DefaultRelationshipGroupCursor AllocateRelationshipGroupCursor()
		 {
			  if ( _relationshipGroupCursor == null )
			  {
					return Trace( new DefaultRelationshipGroupCursor( this, _storageReader.allocateRelationshipGroupCursor() ) );
			  }

			  try
			  {
					return _relationshipGroupCursor;
			  }
			  finally
			  {
					_relationshipGroupCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultRelationshipGroupCursor cursor )
		 {
			  if ( _relationshipGroupCursor != null )
			  {
					_relationshipGroupCursor.release();
			  }
			  _relationshipGroupCursor = cursor;
		 }

		 public override DefaultNodeValueIndexCursor AllocateNodeValueIndexCursor()
		 {
			  if ( _nodeValueIndexCursor == null )
			  {
					return Trace( new DefaultNodeValueIndexCursor( this ) );
			  }

			  try
			  {
					return _nodeValueIndexCursor;
			  }
			  finally
			  {
					_nodeValueIndexCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultNodeValueIndexCursor cursor )
		 {
			  if ( _nodeValueIndexCursor != null )
			  {
					_nodeValueIndexCursor.release();
			  }
			  _nodeValueIndexCursor = cursor;
		 }

		 public override DefaultNodeLabelIndexCursor AllocateNodeLabelIndexCursor()
		 {
			  if ( _nodeLabelIndexCursor == null )
			  {
					return Trace( new DefaultNodeLabelIndexCursor( this ) );
			  }

			  try
			  {
					return _nodeLabelIndexCursor;
			  }
			  finally
			  {
					_nodeLabelIndexCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultNodeLabelIndexCursor cursor )
		 {
			  if ( _nodeLabelIndexCursor != null )
			  {
					_nodeLabelIndexCursor.release();
			  }
			  _nodeLabelIndexCursor = cursor;
		 }

		 public override DefaultNodeExplicitIndexCursor AllocateNodeExplicitIndexCursor()
		 {
			  if ( _nodeExplicitIndexCursor == null )
			  {
					return Trace( new DefaultNodeExplicitIndexCursor( this ) );
			  }

			  try
			  {
					return _nodeExplicitIndexCursor;
			  }
			  finally
			  {
					_nodeExplicitIndexCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultNodeExplicitIndexCursor cursor )
		 {
			  if ( _nodeExplicitIndexCursor != null )
			  {
					_nodeExplicitIndexCursor.release();
			  }
			  _nodeExplicitIndexCursor = cursor;
		 }

		 public override DefaultRelationshipExplicitIndexCursor AllocateRelationshipExplicitIndexCursor()
		 {
			  if ( _relationshipExplicitIndexCursor == null )
			  {
					return Trace( new DefaultRelationshipExplicitIndexCursor( new DefaultRelationshipScanCursor( null, _storageReader.allocateRelationshipScanCursor() ), this ) );
			  }

			  try
			  {
					return _relationshipExplicitIndexCursor;
			  }
			  finally
			  {
					_relationshipExplicitIndexCursor = null;
			  }
		 }

		 public virtual void Accept( DefaultRelationshipExplicitIndexCursor cursor )
		 {
			  if ( _relationshipExplicitIndexCursor != null )
			  {
					_relationshipExplicitIndexCursor.release();
			  }
			  _relationshipExplicitIndexCursor = cursor;
		 }

		 public virtual void Release()
		 {
			  if ( _nodeCursor != null )
			  {
					_nodeCursor.release();
					_nodeCursor = null;
			  }
			  if ( _relationshipScanCursor != null )
			  {
					_relationshipScanCursor.release();
					_relationshipScanCursor = null;
			  }
			  if ( _relationshipTraversalCursor != null )
			  {
					_relationshipTraversalCursor.release();
					_relationshipTraversalCursor = null;
			  }
			  if ( _propertyCursor != null )
			  {
					_propertyCursor.release();
					_propertyCursor = null;
			  }
			  if ( _relationshipGroupCursor != null )
			  {
					_relationshipGroupCursor.release();
					_relationshipGroupCursor = null;
			  }
			  if ( _nodeValueIndexCursor != null )
			  {
					_nodeValueIndexCursor.release();
					_nodeValueIndexCursor = null;
			  }
			  if ( _nodeLabelIndexCursor != null )
			  {
					_nodeLabelIndexCursor.release();
					_nodeLabelIndexCursor = null;
			  }
			  if ( _nodeExplicitIndexCursor != null )
			  {
					_nodeExplicitIndexCursor.release();
					_nodeExplicitIndexCursor = null;
			  }
			  if ( _relationshipExplicitIndexCursor != null )
			  {
					_relationshipExplicitIndexCursor.release();
					_relationshipExplicitIndexCursor = null;
			  }
		 }

		 private T Trace<T>( T closeable ) where T : Neo4Net.Internal.Kernel.Api.IDisposablePlus
		 {
			  if ( _debugClosing )
			  {
					StackTraceElement[] stackTrace = Thread.CurrentThread.StackTrace;
					_closeables.Add( new CloseableStacktrace( closeable, Arrays.copyOfRange( stackTrace, 2, stackTrace.Length ) ) );
			  }
			  return closeable;
		 }

		 internal virtual void AssertClosed()
		 {
			  if ( _debugClosing )
			  {
					foreach ( CloseableStacktrace c in _closeables )
					{
						 c.AssertClosed();
					}
					_closeables.Clear();
			  }
		 }

		 internal class CloseableStacktrace
		 {
			  internal readonly IDisposablePlus C;
			  internal readonly StackTraceElement[] StackTrace;

			  internal CloseableStacktrace( IDisposablePlus c, StackTraceElement[] stackTrace )
			  {
					this.C = c;
					this.StackTrace = stackTrace;
			  }

			  internal virtual void AssertClosed()
			  {
					if ( !C.Closed )
					{
						 MemoryStream @out = new MemoryStream();
						 PrintStream printStream = new PrintStream( @out );

						 foreach ( StackTraceElement traceElement in StackTrace )
						 {
							  printStream.println( "\tat " + traceElement );
						 }
						 printStream.println();
						 throw new System.InvalidOperationException( format( "Closeable %s was not closed!\n%s", C, @out.ToString() ) );
					}
			  }
		 }
	}

}