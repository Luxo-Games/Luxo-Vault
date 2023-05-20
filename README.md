# LuxoVault

LuxoVault: Safely store and manage your data with ease!

LuxoVault is a powerful C# library that empowers developers to securely store and manage data locally or online using various data formats. Whether you need efficient binary serialization with Protobuf or the human-readable integrity of Signed JSON, LuxoVault has got you covered.

## Features

- **Multiple Data Formats**: Choose between Protobuf, a widely-used binary serialization format, or Signed JSON, a human-readable format with built-in data integrity validation.

- **Data Security**: While Protobuf is an efficient serialization format, it does not provide built-in mechanisms for data integrity or encryption. To ensure data integrity, you may consider implementing additional security measures such as digital signatures. LuxoVault is actively exploring options for integrating signature functionality with Protobuf storage.

- **Simplified API**: LuxoVault provides a range of vault classes that implement the `IVault` interface. You can easily access these classes through the interface methods without worrying about complex implementations.

- **Flexible Storage Options**: Customize your storage locations effortlessly. LuxoVault gives you the freedom to choose where you want to store your precious data.

## Installation

LuxoVault can be effortlessly installed as a NuGet package. Official releases will be made available soon.

## Dependencies

LuxoVault relies on the following dependencies:

### LuxoVault.Protobuf

- **Protobuf-net**: A C# library for efficient protobuf serialization.

## System Requirements

LuxoVault has no specific system requirements and can be used on any system capable of running compiled C# code.

## Usage

Getting started with LuxoVault is a breeze:

1. Select the desired vault class based on your preferred data format.
2. LuxoVault provides a range of vault classes that implement the `IVault` interface, allowing you to switch between data storage formats effortlessly.
3. Create your DTO Class to represent the data you want to store. 
4. Store your data securely using the `SaveData` method.
5. Retrieve your data effortlessly using the `LoadData` method.

Please note that data formats are not cross-compatible due to their distinct serialization methods.

To ensure data integrity and protection against tampering with Protobuf storage, LuxoVault is actively exploring options for integrating signature functionality. Stay tuned for future updates on this exciting feature!

## Data Compression and Serialization

To ensure efficient storage, Protobuf-based vault classes in LuxoVault support data compression out of the box.

## Error Handling and Validation

LuxoVault takes care of validating that signed JSON data originates from a trusted source. While no other built-in validation or error handling mechanisms are planned, LuxoVault strives to provide a smooth and reliable experience.

## Customization and Future Enhancements

In future updates, LuxoVault will offer:

- **Comprehensive Documentation**: Detailed documentation and practical usage examples to guide you through LuxoVault's features and functionalities.

- **Flexible Configuration Options**: Customize storage locations and other configurations to suit your specific requirements effortlessly.

- **Expanded Data Formats and Utilities**: Unlock additional data formats and utility functionalities to further enhance your data management capabilities.

## License

LuxoVault is released under the Apache 2.0 License. For more information, please refer to the [LICENSE](LICENSE) file.

## Support

Have questions, issues, or suggestions? We'd love to hear from you! Feel free to create GitHub issues or reach out to us via email at lubber@luxo.games.

Let LuxoVault take care of your data storage needs while you focus on building amazing applications!
