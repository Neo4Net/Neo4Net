using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.protocol
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;
	using JdkZlibDecoder = io.netty.handler.codec.compression.JdkZlibDecoder;
	using JdkZlibEncoder = io.netty.handler.codec.compression.JdkZlibEncoder;
	using Lz4FrameDecoder = io.netty.handler.codec.compression.Lz4FrameDecoder;
	using Lz4FrameEncoder = io.netty.handler.codec.compression.Lz4FrameEncoder;
	using SnappyFrameDecoder = io.netty.handler.codec.compression.SnappyFrameDecoder;
	using SnappyFrameEncoder = io.netty.handler.codec.compression.SnappyFrameEncoder;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_LZ4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_LZ4_HIGH_COMPRESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_LZ4_HIGH_COMPRESSION_VALIDATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_LZ4_VALIDATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_SNAPPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_SNAPPY_VALIDATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_GZIP;

	public interface ModifierProtocolInstaller<O> where O : Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation
	{
		 ICollection<Protocol_ModifierProtocol> Protocols();

		 void apply<BUILDER>( NettyPipelineBuilder<O, BUILDER> nettyPipelineBuilder );
	}

	public static class ModifierProtocolInstaller_Fields
	{
		 public static readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> ServerCompressionInstallers = new IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>>
		 {
			 new ModifierProtocolInstaller_SnappyServer(),
			 new ModifierProtocolInstaller_SnappyValidatingServer(),
			 new ModifierProtocolInstaller_LZ4Server(),
			 new ModifierProtocolInstaller_LZ4ValidatingServer(),
			 new ModifierProtocolInstaller_GzipServer()
		 };
		 public static readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> ClientCompressionInstallers = new IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>>
		 {
			 new ModifierProtocolInstaller_SnappyClient(),
			 new ModifierProtocolInstaller_LZ4Client(),
			 new ModifierProtocolInstaller_LZ4HighCompressionClient(),
			 new ModifierProtocolInstaller_GzipClient()
		 };
		 public static readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> AllClientInstallers = ClientCompressionInstallers;
		 public static readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> AllServerInstallers = ServerCompressionInstallers;
	}

	 public abstract class ModifierProtocolInstaller_BaseClientModifier : ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>
	 {
		  internal readonly string PipelineEncoderName;
		  internal readonly System.Func<MessageToByteEncoder<ByteBuf>> Encoder;
		  internal readonly ICollection<Protocol_ModifierProtocol> ModifierProtocols;

		  protected internal ModifierProtocolInstaller_BaseClientModifier( string pipelineEncoderName, System.Func<MessageToByteEncoder<ByteBuf>> encoder, params Protocol_ModifierProtocol[] modifierProtocols )
		  {
				this.PipelineEncoderName = pipelineEncoderName;
				this.Encoder = encoder;
				this.ModifierProtocols = asList( modifierProtocols );
		  }

		  public override ICollection<Protocol_ModifierProtocol> Protocols()
		  {
				return ModifierProtocols;
		  }

		  public override void Apply<BUILDER>( NettyPipelineBuilder<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client, BUILDER> nettyPipelineBuilder ) where BUILDER : NettyPipelineBuilder<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client,BUILDER>
		  {
				nettyPipelineBuilder.Add( PipelineEncoderName, Encoder.get() );
		  }
	 }

	 public abstract class ModifierProtocolInstaller_BaseServerModifier : ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>
	 {
		  internal readonly string PipelineDecoderName;
		  internal readonly System.Func<ByteToMessageDecoder> Decoder;
		  internal readonly ICollection<Protocol_ModifierProtocol> ModifierProtocols;

		  protected internal ModifierProtocolInstaller_BaseServerModifier( string pipelineDecoderName, System.Func<ByteToMessageDecoder> decoder, params Protocol_ModifierProtocol[] modifierProtocols )
		  {
				this.PipelineDecoderName = pipelineDecoderName;
				this.Decoder = decoder;
				this.ModifierProtocols = asList( modifierProtocols );
		  }

		  public override ICollection<Protocol_ModifierProtocol> Protocols()
		  {
				return ModifierProtocols;
		  }

		  public override void Apply<BUILDER>( NettyPipelineBuilder<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server, BUILDER> nettyPipelineBuilder ) where BUILDER : NettyPipelineBuilder<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server,BUILDER>
		  {
				nettyPipelineBuilder.Add( PipelineDecoderName, Decoder.get() );
		  }
	 }

	 public class ModifierProtocolInstaller_SnappyClient : ModifierProtocolInstaller_BaseClientModifier
	 {
		  internal ModifierProtocolInstaller_SnappyClient() : base("snappy_encoder", SnappyFrameEncoder::new, COMPRESSION_SNAPPY, COMPRESSION_SNAPPY_VALIDATING)
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		  }
	 }

	 public class ModifierProtocolInstaller_SnappyServer : ModifierProtocolInstaller_BaseServerModifier
	 {
		  internal ModifierProtocolInstaller_SnappyServer() : base("snappy_decoder", SnappyFrameDecoder::new, COMPRESSION_SNAPPY)
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		  }
	 }

	 public class ModifierProtocolInstaller_SnappyValidatingServer : ModifierProtocolInstaller_BaseServerModifier
	 {
		  internal ModifierProtocolInstaller_SnappyValidatingServer() : base("snappy_validating_decoder", () -> new SnappyFrameDecoder(true), COMPRESSION_SNAPPY_VALIDATING)
		  {
		  }
	 }

	 public class ModifierProtocolInstaller_LZ4Client : ModifierProtocolInstaller_BaseClientModifier
	 {
		  internal ModifierProtocolInstaller_LZ4Client() : base("lz4_encoder", Lz4FrameEncoder::new, COMPRESSION_LZ4, COMPRESSION_LZ4_VALIDATING)
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		  }
	 }

	 public class ModifierProtocolInstaller_LZ4HighCompressionClient : ModifierProtocolInstaller_BaseClientModifier
	 {
		  internal ModifierProtocolInstaller_LZ4HighCompressionClient() : base("lz4_encoder", () -> new Lz4FrameEncoder(true), COMPRESSION_LZ4_HIGH_COMPRESSION, COMPRESSION_LZ4_HIGH_COMPRESSION_VALIDATING)
		  {
		  }
	 }

	 public class ModifierProtocolInstaller_LZ4Server : ModifierProtocolInstaller_BaseServerModifier
	 {
		  internal ModifierProtocolInstaller_LZ4Server() : base("lz4_decoder", Lz4FrameDecoder::new, COMPRESSION_LZ4, COMPRESSION_LZ4_HIGH_COMPRESSION)
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		  }
	 }

	 public class ModifierProtocolInstaller_LZ4ValidatingServer : ModifierProtocolInstaller_BaseServerModifier
	 {
		  internal ModifierProtocolInstaller_LZ4ValidatingServer() : base("lz4_decoder", () -> new Lz4FrameDecoder(true), COMPRESSION_LZ4_VALIDATING, COMPRESSION_LZ4_HIGH_COMPRESSION_VALIDATING)
		  {
		  }
	 }

	 public class ModifierProtocolInstaller_GzipClient : ModifierProtocolInstaller_BaseClientModifier
	 {
		  internal ModifierProtocolInstaller_GzipClient() : base("zlib_encoder", JdkZlibEncoder::new, COMPRESSION_GZIP)
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		  }
	 }

	 public class ModifierProtocolInstaller_GzipServer : ModifierProtocolInstaller_BaseServerModifier
	 {
		  internal ModifierProtocolInstaller_GzipServer() : base("zlib_decoder", JdkZlibDecoder::new, COMPRESSION_GZIP)
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		  }
	 }

}