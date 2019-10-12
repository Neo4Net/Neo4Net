/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.cluster.protocol.snapshot
{

	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;

	/// <summary>
	/// TODO
	/// </summary>
	public enum SnapshotMessage
	{
		 Join,
		 Leave,
		 SetSnapshotProvider,
		 RefreshSnapshot,
		 SendSnapshot,
		 Snapshot

		 // TODO This needs to be replaced with something that can handle bigger snapshots
//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class SnapshotState implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = 1518479578399690929L;
	//
	//		  private long lastDeliveredInstanceId = -1;
	//		  transient SnapshotProvider provider;
	//
	//		  private transient ObjectOutputStreamFactory objectOutputStreamFactory;
	//
	//		  transient byte[] buf;
	//
	//		  public SnapshotState(long lastDeliveredInstanceId, SnapshotProvider provider, ObjectOutputStreamFactory objectOutputStreamFactory)
	//		  {
	//				this.lastDeliveredInstanceId = lastDeliveredInstanceId;
	//				this.provider = provider;
	//
	//				if (objectOutputStreamFactory == null)
	//				{
	//					 throw new RuntimeException("objectOutputStreamFactory was null");
	//				}
	//
	//				this.objectOutputStreamFactory = objectOutputStreamFactory;
	//		  }
	//
	//		  public long getLastDeliveredInstanceId()
	//		  {
	//				return lastDeliveredInstanceId;
	//		  }
	//
	//		  public void setState(SnapshotProvider provider, ObjectInputStreamFactory objectInputStreamFactory)
	//		  {
	//				ByteArrayInputStream bin = new ByteArrayInputStream(buf);
	//
	//				try (ObjectInputStream oin = objectInputStreamFactory.create(bin))
	//				{
	//					 provider.setState(oin);
	//				}
	//				catch (Throwable e)
	//				{
	//					 e.printStackTrace();
	//				}
	//		  }
	//
	//		  private void writeObject(java.io.ObjectOutputStream @out) throws IOException
	//		  {
	//				@out.defaultWriteObject();
	//				ByteArrayOutputStream bout = new ByteArrayOutputStream();
	//				ObjectOutputStream oout = objectOutputStreamFactory.create(bout);
	//				provider.getState(oout);
	//				oout.close();
	//				byte[] buf = bout.toByteArray();
	//				@out.writeInt(buf.length);
	//				@out.write(buf);
	//		  }
	//
	//		  private void readObject(java.io.ObjectInputStream @in) throws IOException, ClassNotFoundException
	//		  {
	//				@in.defaultReadObject();
	//				buf = new byte[@in.readInt()];
	//				try
	//				{
	//					 @in.readFully(buf);
	//				}
	//				catch (EOFException endOfFile)
	//				{
	//					 // do nothing - the stream's ended but the message content got through ok.
	//				}
	//		  }
	//	 }
	}

}